using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>Identifies a <see cref="Venue"/>.</summary>
public readonly record struct VenueId(Guid Value) : IStronglyTypedId<VenueId>
{
    /// <inheritdoc />
    public static VenueId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static VenueId New() => new(Guid.CreateVersion7());
}
