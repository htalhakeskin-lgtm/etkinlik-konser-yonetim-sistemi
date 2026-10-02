using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Catalog.Domain.Models;

/// <summary>Identifies an <see cref="EquipmentModel"/>.</summary>
public readonly record struct EquipmentModelId(Guid Value) : IStronglyTypedId<EquipmentModelId>
{
    /// <inheritdoc />
    public static EquipmentModelId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static EquipmentModelId New() => new(Guid.CreateVersion7());
}
