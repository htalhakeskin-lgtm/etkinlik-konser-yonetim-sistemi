using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Records a venue with its technical details (US-VEN-001).</summary>
public sealed record CreateVenueCommand(VenueDescription Description) : ICommand<VenueId>, IVenueCommand;
