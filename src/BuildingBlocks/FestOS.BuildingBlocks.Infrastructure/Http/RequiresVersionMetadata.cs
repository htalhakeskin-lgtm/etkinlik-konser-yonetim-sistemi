namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Marks an endpoint that changes an aggregate and so requires <c>If-Match</c>; the OpenAPI document
/// shows the header as required from it (api §9, AT-15).
/// </summary>
public sealed class RequiresVersionMetadata
{
    /// <summary>The single instance.</summary>
    public static readonly RequiresVersionMetadata Instance = new();

    private RequiresVersionMetadata() { }
}
