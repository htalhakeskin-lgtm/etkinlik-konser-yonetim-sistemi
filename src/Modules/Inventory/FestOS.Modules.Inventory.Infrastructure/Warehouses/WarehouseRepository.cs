using FestOS.BuildingBlocks.Infrastructure.Locking;
using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

internal sealed class WarehouseRepository(InventoryDbContext context) : IWarehouseRepository
{
    // The key of the lock under which the active warehouses are counted (database §11.3).
    private const string ActiveWarehousesLock = "inventory:warehouses:active";

    public Task<Warehouse?> FindAsync(WarehouseId id, CancellationToken cancellationToken) =>
        context.Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == id, cancellationToken);

    public async Task<bool> AnyOtherActiveAsync(WarehouseId except, CancellationToken cancellationToken)
    {
        await AdvisoryLocks.AcquireTransactionLocksAsync(context.Database, [ActiveWarehousesLock], cancellationToken);
        return await context.Warehouses.AnyAsync(
            warehouse => warehouse.Id != except && warehouse.DeactivatedAt == null,
            cancellationToken
        );
    }

    public void Add(Warehouse warehouse) => context.Warehouses.Add(warehouse);
}
