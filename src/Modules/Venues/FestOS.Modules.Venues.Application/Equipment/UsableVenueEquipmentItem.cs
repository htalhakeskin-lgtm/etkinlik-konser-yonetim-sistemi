using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>A line and how many of it can be used in the asked days; free descriptions are never counted.</summary>
public sealed record UsableVenueEquipmentItem(
    VenueEquipmentId Id,
    string Name,
    bool IsCounted,
    int Quantity,
    int UsableQuantity
);
