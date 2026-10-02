using System.Security.Claims;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Riders.Contracts;

namespace FestOS.Modules.Riders.Infrastructure.Riders;

/// <summary>A user who may see riders may follow a rider's new versions (riders §7).</summary>
internal sealed class RiderGroupPolicy : IRealtimeGroupPolicy
{
    public string GroupType => "riders";

    public ValueTask<bool> CanJoinAsync(ClaimsPrincipal user, string? id, CancellationToken cancellationToken) =>
        ValueTask.FromResult(user.HasClaim(PermissionClaims.Type, RidersPermissions.ViewRiders));
}
