using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>A venue as its page shows it; <see cref="Version"/> is also the <c>ETag</c> (api §9).</summary>
public sealed record VenueDetails(
    VenueId Id,
    string Name,
    string City,
    string Address,
    Guid? OperatorPartyId,
    string? OperatorName,
    bool IsOperatorActive,
    int Capacity,
    decimal? StageWidthMeters,
    decimal? StageDepthMeters,
    decimal? StageHeightMeters,
    string? LoadingDock,
    decimal? PowerCapacityAmperes,
    TimeOnly? Curfew,
    string TimeZone,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
