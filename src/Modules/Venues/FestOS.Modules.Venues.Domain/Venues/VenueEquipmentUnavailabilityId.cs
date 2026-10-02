using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>Identifies a <see cref="VenueEquipmentUnavailability"/>.</summary>
public readonly record struct VenueEquipmentUnavailabilityId(Guid Value)
    : IStronglyTypedId<VenueEquipmentUnavailabilityId>
{
    /// <inheritdoc />
    public static VenueEquipmentUnavailabilityId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static VenueEquipmentUnavailabilityId New() => new(Guid.CreateVersion7());
}
