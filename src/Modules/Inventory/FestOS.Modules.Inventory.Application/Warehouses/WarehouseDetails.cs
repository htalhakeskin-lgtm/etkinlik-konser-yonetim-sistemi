using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>A warehouse as the edit dialog shows it; <see cref="Version"/> is also the <c>ETag</c> (api §9).</summary>
public sealed record WarehouseDetails(
    WarehouseId Id,
    string Name,
    string City,
    string Address,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
