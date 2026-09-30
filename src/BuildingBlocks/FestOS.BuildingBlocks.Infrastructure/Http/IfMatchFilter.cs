using System.Globalization;
using FestOS.BuildingBlocks.Application.Concurrency;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Requires <c>If-Match</c> on an endpoint that changes an aggregate and puts the version in the request's
/// <see cref="ExpectedVersion"/>; the handler compares it after loading the aggregate (api §9). A missing
/// header is <c>428</c>. Only one strong entity tag holding the version is accepted, e.g. <c>"7"</c>:
/// <c>*</c>, weak tags and lists would let a change skip the check.
/// </summary>
internal sealed class IfMatchFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        StringValues header = context.HttpContext.Request.Headers.IfMatch;
        if (StringValues.IsNullOrEmpty(header))
        {
            throw new RequestHeaderException(
                StatusCodes.Status428PreconditionRequired,
                "version-required",
                "Version required",
                ProblemCodes.VersionRequired,
                "The request must send the version it is based on in If-Match, e.g. If-Match: \"7\"."
            );
        }

        context.HttpContext.RequestServices.GetRequiredService<ExpectedVersion>().Set(Parse(header));
        return next(context);
    }

    private static int Parse(StringValues header)
    {
        string value = header.Count == 1 ? header.ToString().Trim() : string.Empty;
        return
            value.Length > 2
            && value[0] == '"'
            && value[^1] == '"'
            && int.TryParse(
                value.AsSpan(1, value.Length - 2),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int version
            )
            ? version
            : throw new RequestHeaderException(
                StatusCodes.Status400BadRequest,
                "malformed-request",
                "Malformed request",
                ProblemCodes.MalformedRequest,
                "If-Match must hold one version as a strong entity tag, e.g. If-Match: \"7\"."
            );
    }
}
