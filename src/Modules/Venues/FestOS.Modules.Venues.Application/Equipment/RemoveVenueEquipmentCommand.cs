using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Removes a line entered by mistake (venues VN-03).</summary>
public sealed record RemoveVenueEquipmentCommand(VenueId VenueId, VenueEquipmentId LineId) : ICommand<bool>;
