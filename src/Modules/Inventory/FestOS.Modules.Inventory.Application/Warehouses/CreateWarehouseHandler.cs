using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class CreateWarehouseHandler(IWarehouseRepository warehouses)
    : ICommandHandler<CreateWarehouseCommand, WarehouseId>
{
    public Task<WarehouseId> HandleAsync(CreateWarehouseCommand command, CancellationToken cancellationToken)
    {
        var warehouse = Warehouse.Create(command.Name, command.City, command.Address);
        warehouses.Add(warehouse);
        return Task.FromResult(warehouse.Id);
    }
}
