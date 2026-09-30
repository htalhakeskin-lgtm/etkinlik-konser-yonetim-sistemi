using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>The endpoint helper of idempotency keys (api §10, ADR-0025).</summary>
public static class IdempotencyEndpointExtensions
{
    /// <summary>
    /// Requires <c>Idempotency-Key</c> on the requests that change data (<c>POST</c>, <c>PUT</c>,
    /// <c>PATCH</c>, <c>DELETE</c>). <c>MapModules</c> adds it to the group of every module endpoint, so it
    /// runs before the endpoints' own filters: a retry reaches its stored result before <c>If-Match</c> is
    /// checked.
    /// </summary>
    public static TBuilder RequiresIdempotencyKey<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddEndpointFilter<TBuilder, IdempotencyFilter>();
    }
}
