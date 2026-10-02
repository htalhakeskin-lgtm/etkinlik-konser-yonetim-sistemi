using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>Identifies a <see cref="VenueEquipment"/>.</summary>
public readonly record struct VenueEquipmentId(Guid Value) : IStronglyTypedId<VenueEquipmentId>
{
    /// <inheritdoc />
    public static VenueEquipmentId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static VenueEquipmentId New() => new(Guid.CreateVersion7());
}
