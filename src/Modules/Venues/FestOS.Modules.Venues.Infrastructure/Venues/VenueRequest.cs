using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

/// <summary>
/// The body of <c>POST /api/v1/venues</c> and <c>PUT /api/v1/venues/{venueId}</c>; the technical details may be
/// left out, and the time zone is Istanbul unless given.
/// </summary>
public sealed record VenueRequest(
    string Name,
    string City,
    string Address,
    int Capacity,
    Guid? OperatorPartyId = null,
    decimal? StageWidthMeters = null,
    decimal? StageDepthMeters = null,
    decimal? StageHeightMeters = null,
    string? LoadingDock = null,
    decimal? PowerCapacityAmperes = null,
    TimeOnly? Curfew = null,
    string? TimeZone = null
)
{
    /// <summary>The details as the venue takes them.</summary>
    public VenueDescription Description() =>
        new(
            Name,
            City,
            Address,
            OperatorPartyId,
            Capacity,
            StageWidthMeters,
            StageDepthMeters,
            StageHeightMeters,
            LoadingDock,
            PowerCapacityAmperes,
            Curfew,
            string.IsNullOrWhiteSpace(TimeZone) ? Venue.DefaultTimeZone : TimeZone
        );
}
