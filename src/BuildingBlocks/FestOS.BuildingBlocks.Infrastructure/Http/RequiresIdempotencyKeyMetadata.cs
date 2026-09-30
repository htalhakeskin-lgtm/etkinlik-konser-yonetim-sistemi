namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Marks the endpoints whose changing requests need <c>Idempotency-Key</c>; the OpenAPI document shows
/// the header as required from it (api §10, AT-15).
/// </summary>
public sealed class RequiresIdempotencyKeyMetadata
{
    /// <summary>The single instance.</summary>
    public static readonly RequiresIdempotencyKeyMetadata Instance = new();

    private RequiresIdempotencyKeyMetadata() { }
}
