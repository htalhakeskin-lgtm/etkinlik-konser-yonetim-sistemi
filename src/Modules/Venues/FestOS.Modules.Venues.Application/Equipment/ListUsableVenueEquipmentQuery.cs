using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>
/// How much of each line can be used on the venue's days from <paramref name="From"/> up to, not
/// including, <paramref name="To"/> (US-VEN-002 criterion 5, BR-VEN-001).
/// </summary>
public sealed record ListUsableVenueEquipmentQuery(VenueId VenueId, DateOnly From, DateOnly To)
    : IQuery<IReadOnlyList<UsableVenueEquipmentItem>>;
