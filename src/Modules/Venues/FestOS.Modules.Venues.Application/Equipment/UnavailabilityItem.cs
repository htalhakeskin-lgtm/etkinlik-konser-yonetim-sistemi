using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>An unavailability period; the end day is not included (database §7.3).</summary>
public sealed record UnavailabilityItem(
    VenueEquipmentUnavailabilityId Id,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int Quantity,
    string Reason
);
