using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Catalog.Domain.Categories;

/// <summary>Identifies an <see cref="EquipmentCategory"/>.</summary>
public readonly record struct EquipmentCategoryId(Guid Value) : IStronglyTypedId<EquipmentCategoryId>
{
    /// <inheritdoc />
    public static EquipmentCategoryId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static EquipmentCategoryId New() => new(Guid.CreateVersion7());
}
