namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Marks a changing endpoint whose result must never be replayed, such as signing in: a stored answer
/// would let a retry through after the account was locked or deactivated (identity §7).
/// </summary>
public sealed class NoIdempotencyKeyMetadata
{
    /// <summary>The single instance.</summary>
    public static readonly NoIdempotencyKeyMetadata Instance = new();

    private NoIdempotencyKeyMetadata() { }
}
