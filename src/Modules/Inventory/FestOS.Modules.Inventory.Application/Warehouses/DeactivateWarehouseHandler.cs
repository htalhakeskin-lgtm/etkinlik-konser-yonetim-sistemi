using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Inventory.Domain;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class DeactivateWarehouseHandler(
    IWarehouseRepository warehouses,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateWarehouseCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateWarehouseCommand command, CancellationToken cancellationToken)
    {
        Warehouse warehouse = await warehouses.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        if (warehouse.DeactivatedAt is not null)
        {
            return true;
        }

        if (!await warehouses.AnyOtherActiveAsync(warehouse.Id, cancellationToken))
        {
            throw new BusinessRuleViolationException(
                InventoryRuleCodes.LastActiveWarehouse,
                "The last active warehouse cannot be deactivated."
            );
        }

        warehouse.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
