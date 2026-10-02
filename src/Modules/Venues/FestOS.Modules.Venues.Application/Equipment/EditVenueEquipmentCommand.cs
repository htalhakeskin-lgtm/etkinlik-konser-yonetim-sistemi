using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Changes a venue equipment line (US-VEN-002, BR-VEN-002).</summary>
public sealed record EditVenueEquipmentCommand(VenueId VenueId, VenueEquipmentId LineId, VenueEquipmentDetails Details)
    : ICommand<bool>;
