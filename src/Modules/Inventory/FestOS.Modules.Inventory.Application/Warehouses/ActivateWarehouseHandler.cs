using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class ActivateWarehouseHandler(IWarehouseRepository warehouses, ExpectedVersion expectedVersion)
    : ICommandHandler<ActivateWarehouseCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateWarehouseCommand command, CancellationToken cancellationToken)
    {
        Warehouse warehouse = await warehouses.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        warehouse.Activate();
        return true;
    }
}
