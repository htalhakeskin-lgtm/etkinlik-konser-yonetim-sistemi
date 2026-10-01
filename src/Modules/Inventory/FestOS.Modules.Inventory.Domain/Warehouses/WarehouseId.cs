using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Inventory.Domain.Warehouses;

/// <summary>Identifies a <see cref="Warehouse"/>.</summary>
public readonly record struct WarehouseId(Guid Value) : IStronglyTypedId<WarehouseId>
{
    /// <inheritdoc />
    public static WarehouseId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static WarehouseId New() => new(Guid.CreateVersion7());
}
