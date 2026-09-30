using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// An endpoint result that returns an aggregate, with its version in <c>ETag: "7"</c> (api §9). The inner
/// result's OpenAPI metadata is kept.
/// </summary>
/// <typeparam name="TResult">The result that writes the body, e.g. <c>Ok&lt;T&gt;</c>.</typeparam>
public sealed class VersionedResult<TResult>(TResult inner, int version) : IResult, IEndpointMetadataProvider
    where TResult : IResult, IEndpointMetadataProvider
{
    /// <summary>The result that writes the body.</summary>
    public TResult Inner { get; } = inner;

    /// <summary>The aggregate's version.</summary>
    public int Version { get; } = version;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        httpContext.Response.Headers.ETag = $"\"{Version.ToString(CultureInfo.InvariantCulture)}\"";
        return Inner.ExecuteAsync(httpContext);
    }

    /// <inheritdoc />
    [SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "IEndpointMetadataProvider requires the static method, as on the framework's own results."
    )]
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        TResult.PopulateMetadata(method, builder);
}
