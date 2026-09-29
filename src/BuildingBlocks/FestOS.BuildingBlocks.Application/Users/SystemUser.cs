namespace FestOS.BuildingBlocks.Application.Users;

/// <summary>
/// The fixed user that records work no person started, so <c>…_by</c> columns are never empty. The
/// Identity module seeds a matching user that cannot sign in (database §9, §16.4).
/// </summary>
public static class SystemUser
{
    /// <summary>The system user's identifier; laid out as a version 7 UUID with a zero timestamp.</summary>
    public static readonly Guid Id = new("00000000-0000-7000-8000-000000000001");
}
