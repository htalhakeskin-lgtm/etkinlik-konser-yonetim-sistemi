using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Loads and adds warehouses for commands (identity ID-01); the unit of work saves them.</summary>
public interface IWarehouseRepository
{
    /// <summary>The warehouse with the identifier, or <see langword="null"/>.</summary>
    Task<Warehouse?> FindAsync(WarehouseId id, CancellationToken cancellationToken);

    /// <summary>Adds a new warehouse.</summary>
    void Add(Warehouse warehouse);
}
