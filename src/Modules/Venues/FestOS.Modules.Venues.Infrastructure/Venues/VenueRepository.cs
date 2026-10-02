using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

internal sealed class VenueRepository(VenuesDbContext context) : IVenueRepository
{
    public Task<Venue?> FindAsync(VenueId id, CancellationToken cancellationToken) =>
        context.Venues.SingleOrDefaultAsync(venue => venue.Id == id, cancellationToken);

    public void Add(Venue venue) => context.Venues.Add(venue);
}
