using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

internal sealed class WarehouseRepository(InventoryDbContext context) : IWarehouseRepository
{
    public Task<Warehouse?> FindAsync(WarehouseId id, CancellationToken cancellationToken) =>
        context.Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == id, cancellationToken);

    public void Add(Warehouse warehouse) => context.Warehouses.Add(warehouse);
}
