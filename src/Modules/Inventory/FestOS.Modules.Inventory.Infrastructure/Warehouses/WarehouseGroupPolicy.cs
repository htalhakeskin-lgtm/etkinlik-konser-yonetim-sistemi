using System.Security.Claims;
using FestOS.BuildingBlocks.Infrastructure.Realtime;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

/// <summary>Every signed-in user may follow the warehouses, since the list is open to all (inventory §6).</summary>
internal sealed class WarehouseGroupPolicy : IRealtimeGroupPolicy
{
    public string GroupType => "warehouses";

    public ValueTask<bool> CanJoinAsync(ClaimsPrincipal user, string? id, CancellationToken cancellationToken) =>
        ValueTask.FromResult(true);
}
