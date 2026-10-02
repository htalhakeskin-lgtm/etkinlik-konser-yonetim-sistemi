using System.Security.Claims;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Venues.Contracts;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

/// <summary>A user who may see venues may follow a venue's changes (venues §7).</summary>
internal sealed class VenueGroupPolicy : IRealtimeGroupPolicy
{
    public string GroupType => "venues";

    public ValueTask<bool> CanJoinAsync(ClaimsPrincipal user, string? id, CancellationToken cancellationToken) =>
        ValueTask.FromResult(user.HasClaim(PermissionClaims.Type, VenuesPermissions.ViewVenues));
}
