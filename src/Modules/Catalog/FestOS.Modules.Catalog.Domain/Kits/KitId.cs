using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Catalog.Domain.Kits;

/// <summary>Identifies a <see cref="Kit"/>.</summary>
public readonly record struct KitId(Guid Value) : IStronglyTypedId<KitId>
{
    /// <inheritdoc />
    public static KitId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static KitId New() => new(Guid.CreateVersion7());
}
