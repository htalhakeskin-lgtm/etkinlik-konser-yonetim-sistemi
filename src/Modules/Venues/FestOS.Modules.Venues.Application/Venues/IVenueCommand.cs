using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>What creating and editing a venue both send, so one set of checks covers both (venues §6).</summary>
public interface IVenueCommand
{
    /// <summary>The venue's details.</summary>
    VenueDescription Description { get; }
}
