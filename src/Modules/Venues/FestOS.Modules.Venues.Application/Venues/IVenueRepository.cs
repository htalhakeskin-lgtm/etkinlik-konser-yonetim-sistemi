using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Loads and adds venues for commands (identity ID-01); the unit of work saves them.</summary>
public interface IVenueRepository
{
    /// <summary>The venue, or <see langword="null"/>.</summary>
    Task<Venue?> FindAsync(VenueId id, CancellationToken cancellationToken);

    /// <summary>Adds a new venue.</summary>
    void Add(Venue venue);
}
