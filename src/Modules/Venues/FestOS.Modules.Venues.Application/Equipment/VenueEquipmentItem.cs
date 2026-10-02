using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>An equipment line as the venue page shows it; <see cref="IsCounted"/> is false for free descriptions.</summary>
public sealed record VenueEquipmentItem(
    VenueEquipmentId Id,
    Guid? ModelId,
    Guid? CategoryId,
    string? Description,
    string Name,
    IReadOnlyList<string> CategoryPath,
    bool IsTargetActive,
    bool IsCounted,
    int Quantity,
    DateOnly? ValidityStart,
    DateOnly? ValidityEnd,
    IReadOnlyList<UnavailabilityItem> Unavailabilities
);
