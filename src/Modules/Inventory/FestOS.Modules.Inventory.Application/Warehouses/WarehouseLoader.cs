using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Loads the warehouse a command changes, at the version the request saw (api §9).</summary>
internal static class WarehouseLoader
{
    public static async Task<Warehouse> LoadForChangeAsync(
        this IWarehouseRepository warehouses,
        WarehouseId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        Warehouse warehouse =
            await warehouses.FindAsync(id, cancellationToken) ?? throw new NotFoundException("Warehouse", id.Value);
        expectedVersion.EnsureMatches(warehouse);
        return warehouse;
    }
}
