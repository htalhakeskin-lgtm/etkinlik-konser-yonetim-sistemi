using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Records days when part of a line cannot be used (US-VEN-002).</summary>
public sealed record AddVenueEquipmentUnavailabilityCommand(
    VenueId VenueId,
    VenueEquipmentId LineId,
    UnavailabilityDetails Details
) : ICommand<VenueEquipmentUnavailabilityId>;
