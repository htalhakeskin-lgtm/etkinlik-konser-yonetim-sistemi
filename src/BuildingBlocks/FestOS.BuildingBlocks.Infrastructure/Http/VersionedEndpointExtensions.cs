using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>The endpoint helpers of optimistic concurrency over HTTP (api §9, ADR-0024).</summary>
public static class VersionedEndpointExtensions
{
    /// <summary>
    /// Declares that the endpoint changes an aggregate: <c>If-Match</c> becomes required and its version
    /// reaches the handler through <c>ExpectedVersion</c>.
    /// </summary>
    public static TBuilder RequiresVersion<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(RequiresVersionMetadata.Instance).AddEndpointFilter<TBuilder, IfMatchFilter>();
    }

    /// <summary>Adds the aggregate's version to the response as <c>ETag: "{version}"</c>.</summary>
    public static VersionedResult<TResult> WithVersion<TResult>(this TResult result, int version)
        where TResult : IResult, IEndpointMetadataProvider => new(result, version);
}
