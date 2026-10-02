using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>One venue with its operator's name, for its page and its form.</summary>
public sealed record GetVenueQuery(VenueId Id) : IQuery<VenueDetails>;
