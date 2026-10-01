using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>A row of the warehouses list; <c>Version</c> lets the row's actions send <c>If-Match</c> (api §9).</summary>
public sealed record WarehouseListItem(
    WarehouseId Id,
    string Name,
    string City,
    string Address,
    bool IsActive,
    int Version
);
