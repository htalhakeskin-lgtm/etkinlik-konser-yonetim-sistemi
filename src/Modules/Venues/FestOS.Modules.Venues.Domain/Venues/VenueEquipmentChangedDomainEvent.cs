using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>
/// A venue's equipment changed; the days run from <see cref="AffectedStart"/> up to, not including,
/// <see cref="AffectedEnd"/>, either open when missing.
/// </summary>
public sealed record VenueEquipmentChangedDomainEvent(VenueId VenueId, DateOnly? AffectedStart, DateOnly? AffectedEnd)
    : IDomainEvent;
