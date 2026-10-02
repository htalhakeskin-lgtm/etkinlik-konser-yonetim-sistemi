using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Changes a venue's details (US-VEN-001).</summary>
public sealed record EditVenueCommand(VenueId Id, VenueDescription Description) : ICommand<bool>, IVenueCommand;
