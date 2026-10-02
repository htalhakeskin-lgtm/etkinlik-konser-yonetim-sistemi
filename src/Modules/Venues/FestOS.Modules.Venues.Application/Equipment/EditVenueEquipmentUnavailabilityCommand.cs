using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Changes an unavailability period (BR-VEN-002).</summary>
public sealed record EditVenueEquipmentUnavailabilityCommand(
    VenueId VenueId,
    VenueEquipmentId LineId,
    VenueEquipmentUnavailabilityId PeriodId,
    UnavailabilityDetails Details
) : ICommand<bool>;
