using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Takes a venue out of new selections (BR-SYS-001).</summary>
public sealed record DeactivateVenueCommand(VenueId Id) : ICommand<bool>;
