using System.Security.Cryptography;
using System.Text;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Requires <c>Idempotency-Key</c> on every request that changes data and puts the key and the request's
/// fingerprint in the request's <see cref="IdempotencyRequest"/>, for the unit of work of the request's
/// first command (api §10). A response built from a stored result gets <c>Idempotency-Replayed: true</c>.
/// </summary>
internal sealed class IdempotencyFilter : IEndpointFilter
{
    public const string KeyHeader = "Idempotency-Key";

    public const string ReplayedHeader = "Idempotency-Replayed";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        HttpContext httpContext = context.HttpContext;
        if (!ChangesData(httpContext.Request.Method))
        {
            return await next(context);
        }

        IdempotencyRequest idempotency = httpContext.RequestServices.GetRequiredService<IdempotencyRequest>();
        idempotency.Set(
            ReadKey(httpContext.Request.Headers[KeyHeader]),
            await FingerprintAsync(httpContext.Request, httpContext.RequestAborted)
        );

        object? result = await next(context);
        if (idempotency.Replayed)
        {
            httpContext.Response.Headers[ReplayedHeader] = "true";
        }

        return result;
    }

    public static bool ChangesData(string method) =>
        HttpMethods.IsPost(method)
        || HttpMethods.IsPut(method)
        || HttpMethods.IsPatch(method)
        || HttpMethods.IsDelete(method);

    private static Guid ReadKey(StringValues header)
    {
        if (StringValues.IsNullOrEmpty(header))
        {
            throw new RequestHeaderException(
                StatusCodes.Status400BadRequest,
                "idempotency-key-missing",
                "Idempotency key missing",
                ProblemCodes.IdempotencyKeyMissing,
                "The request must send a new UUID in Idempotency-Key, and the same one when it is retried."
            );
        }

        return header.Count == 1 && Guid.TryParseExact(header.ToString(), "D", out Guid key)
            ? key
            : throw new RequestHeaderException(
                StatusCodes.Status400BadRequest,
                "malformed-request",
                "Malformed request",
                ProblemCodes.MalformedRequest,
                "Idempotency-Key must be one UUID, e.g. 0192f0a0-0000-7000-8000-000000000001."
            );
    }

    // The method, the address and the SHA-256 of the body as sent; a retry sends the same bytes (api §10).
    private static async Task<string> FingerprintAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!request.Body.CanSeek)
        {
            throw new InvalidOperationException(
                "The request body is not buffered for the idempotency fingerprint; call UseHttpPlatform first."
            );
        }

        request.Body.Position = 0;
        byte[] body = await SHA256.HashDataAsync(request.Body, cancellationToken);
        request.Body.Position = 0;

        string fingerprint = $"{request.Method}\n{request.Path}{request.QueryString}\n{Convert.ToHexStringLower(body)}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint)));
    }
}
