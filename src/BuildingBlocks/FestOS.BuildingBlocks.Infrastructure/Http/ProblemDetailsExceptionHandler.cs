using System.Globalization;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Rules;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// The single place where exceptions become responses (api §8.3). Endpoints never build error
/// responses; they throw, and this writes RFC 9457 Problem Details with <c>code</c>, <c>params</c>,
/// <c>errors</c> and <c>traceId</c>. Expected outcomes are logged by the decorators; only unexpected
/// errors are logged here.
/// </summary>
internal sealed partial class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<ProblemDetailsExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        ProblemDetails problem = exception switch
        {
            ValidationFailedException validation => Problem(
                StatusCodes.Status400BadRequest,
                "validation",
                "Validation failed",
                ErrorCodes.Validation,
                exception.Message,
                errors: ValidationProblemErrors.From(validation.Errors)
            ),
            RequestHeaderException header => Problem(
                header.StatusCode,
                header.Type,
                header.Title,
                header.Code,
                header.Message
            ),
            BadHttpRequestException badRequest => Problem(
                badRequest.StatusCode,
                "malformed-request",
                "Malformed request",
                ProblemCodes.MalformedRequest,
                "The request could not be read."
            ),
            BusinessRuleViolationException violation => Problem(
                violation.Kind == RuleKind.Authorization
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status422UnprocessableEntity,
                "business-rule",
                "Business rule violated",
                violation.RuleCode,
                violation.Message,
                violation.Parameters
            ),
            NotFoundException => Problem(
                StatusCodes.Status404NotFound,
                "not-found",
                "Not found",
                ErrorCodes.NotFound,
                exception.Message
            ),
            ConcurrencyConflictException => Problem(
                StatusCodes.Status412PreconditionFailed,
                "concurrency-conflict",
                "Concurrency conflict",
                ErrorCodes.ConcurrencyConflict,
                exception.Message
            ),
            AuthenticationFailedException failed => Problem(
                StatusCodes.Status401Unauthorized,
                "authentication-failed",
                "Authentication failed",
                failed.Code,
                failed.Message,
                failed.Parameters
            ),
            RateLimitedException => Problem(
                StatusCodes.Status429TooManyRequests,
                "rate-limited",
                "Too many requests",
                ErrorCodes.RateLimited,
                exception.Message
            ),
            IdempotencyKeyReusedException => Problem(
                StatusCodes.Status422UnprocessableEntity,
                "idempotency-key-reused",
                "Idempotency key reused",
                ErrorCodes.IdempotencyKeyReused,
                exception.Message
            ),
            IdempotencyKeyInProgressException => Problem(
                StatusCodes.Status409Conflict,
                "idempotency-key-in-progress",
                "Idempotency key in progress",
                ErrorCodes.IdempotencyKeyInProgress,
                exception.Message
            ),
            // Nothing about the failure leaves the server; the trace id links the user to the log.
            _ => Problem(
                StatusCodes.Status500InternalServerError,
                "internal-error",
                "Internal error",
                ProblemCodes.InternalError,
                "An unexpected error occurred."
            ),
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            LogUnexpected(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        }

        if (exception is RateLimitedException limited)
        {
            httpContext.Response.Headers.RetryAfter = Math.Ceiling(limited.RetryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception,
            }
        );
    }

    private static ProblemDetails Problem(
        int status,
        string type,
        string title,
        string code,
        string detail,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyList<ValidationProblemError>? errors = null
    )
    {
        var problem = new ProblemDetails
        {
            Type = ProblemCodes.TypePrefix + type,
            Title = title,
            Status = status,
            Detail = detail,
        };
        problem.Extensions["code"] = code;
        if (parameters is { Count: > 0 })
        {
            problem.Extensions["params"] = parameters;
        }

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return problem;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Request {RequestMethod} {RequestPath} failed unexpectedly")]
    private static partial void LogUnexpected(
        ILogger logger,
        Exception exception,
        string requestMethod,
        string requestPath
    );
}
