using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.IntegrationEvents;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>
/// A deactivated warehouse leaves its warehouse managers (identity §8, ID-09): they stay active and their
/// open sessions lose the warehouse at once; one left without a warehouse is flagged in the users list.
/// </summary>
internal sealed class RemoveWarehouseOnWarehouseDeactivatedHandler(IUserRepository users, IUserSessions sessions)
    : IIntegrationEventHandler<WarehouseDeactivatedIntegrationEvent>
{
    public async Task HandleAsync(
        WarehouseDeactivatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken
    )
    {
        foreach (User user in await users.ListWithWarehouseAsync(integrationEvent.WarehouseId, cancellationToken))
        {
            user.RemoveWarehouse(integrationEvent.WarehouseId);
            await sessions.RefreshAllAsync(user, cancellationToken);
        }
    }
}
