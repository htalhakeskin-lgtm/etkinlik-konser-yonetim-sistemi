using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Catalog.Domain.Kits;

/// <summary>Identifies a <see cref="KitLine"/>.</summary>
public readonly record struct KitLineId(Guid Value) : IStronglyTypedId<KitLineId>
{
    /// <inheritdoc />
    public static KitLineId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static KitLineId New() => new(Guid.CreateVersion7());
}
