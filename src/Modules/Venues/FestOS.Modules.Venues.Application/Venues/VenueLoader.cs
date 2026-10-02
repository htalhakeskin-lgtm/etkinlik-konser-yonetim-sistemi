using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Loads the venue a command changes, at the version the request saw (api §9).</summary>
internal static class VenueLoader
{
    public static async Task<Venue> LoadForChangeAsync(
        this IVenueRepository venues,
        VenueId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        Venue venue = await venues.FindAsync(id, cancellationToken) ?? throw new NotFoundException("Venue", id.Value);
        expectedVersion.EnsureMatches(venue);
        return venue;
    }
}
