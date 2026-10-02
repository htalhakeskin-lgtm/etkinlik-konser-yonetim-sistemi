using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Adds a line to a venue's equipment (US-VEN-002).</summary>
public sealed record AddVenueEquipmentCommand(VenueId VenueId, VenueEquipmentDetails Details)
    : ICommand<VenueEquipmentId>;
