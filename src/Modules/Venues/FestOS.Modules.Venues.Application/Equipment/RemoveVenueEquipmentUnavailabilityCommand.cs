using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Removes an unavailability period.</summary>
public sealed record RemoveVenueEquipmentUnavailabilityCommand(
    VenueId VenueId,
    VenueEquipmentId LineId,
    VenueEquipmentUnavailabilityId PeriodId
) : ICommand<bool>;
