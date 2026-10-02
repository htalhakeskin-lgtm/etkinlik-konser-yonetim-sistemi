using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Opens a deactivated venue again.</summary>
public sealed record ActivateVenueCommand(VenueId Id) : ICommand<bool>;
