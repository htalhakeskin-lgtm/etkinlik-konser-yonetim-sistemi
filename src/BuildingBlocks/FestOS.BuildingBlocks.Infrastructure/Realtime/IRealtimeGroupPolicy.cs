using System.Security.Claims;

namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// Decides who may join the groups of one type, e.g. <c>warehouses</c> for <c>warehouses:{id}</c> (building-blocks
/// §11). A module registers one per group type it notifies; a group without a policy cannot be joined.
/// </summary>
public interface IRealtimeGroupPolicy
{
    /// <summary>The resource part of the group names, e.g. <c>warehouses</c>.</summary>
    string GroupType { get; }

    /// <summary>
    /// Whether the user may join the group; <paramref name="id"/> is the record part, or
    /// <see langword="null"/> for the list group.
    /// </summary>
    ValueTask<bool> CanJoinAsync(ClaimsPrincipal user, string? id, CancellationToken cancellationToken);
}
