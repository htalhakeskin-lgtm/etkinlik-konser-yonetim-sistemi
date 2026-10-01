using FestOS.BuildingBlocks.Domain.Rules;

namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>The machine-readable codes of expected outcomes, as the API returns them in <c>code</c> (api §8).</summary>
public static class ErrorCodes
{
    /// <summary>The request failed validation.</summary>
    public const string Validation = "validation";

    /// <summary>The record was not found.</summary>
    public const string NotFound = "notFound";

    /// <summary>The aggregate changed since it was loaded.</summary>
    public const string ConcurrencyConflict = "concurrencyConflict";

    /// <summary>The idempotency key was used for a different request.</summary>
    public const string IdempotencyKeyReused = "idempotencyKeyReused";

    /// <summary>A request with the same idempotency key is still running.</summary>
    public const string IdempotencyKeyInProgress = "idempotencyKeyInProgress";

    /// <summary>
    /// The code of an expected outcome: the rule number for a business rule violation, a technical
    /// code otherwise. Returns <see langword="null"/> for unexpected exceptions.
    /// </summary>
    public static string? Of(Exception exception) =>
        exception switch
        {
            BusinessRuleViolationException violation => violation.RuleCode,
            ValidationFailedException => Validation,
            NotFoundException => NotFound,
            ConcurrencyConflictException => ConcurrencyConflict,
            IdempotencyKeyReusedException => IdempotencyKeyReused,
            IdempotencyKeyInProgressException => IdempotencyKeyInProgress,
            AuthenticationFailedException failed => failed.Code,
            _ => null,
        };
}
