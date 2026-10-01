using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class EditWarehouseHandler(IWarehouseRepository warehouses, ExpectedVersion expectedVersion)
    : ICommandHandler<EditWarehouseCommand, bool>
{
    public async Task<bool> HandleAsync(EditWarehouseCommand command, CancellationToken cancellationToken)
    {
        Warehouse warehouse = await warehouses.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        warehouse.Edit(command.Name, command.City, command.Address);
        return true;
    }
}
