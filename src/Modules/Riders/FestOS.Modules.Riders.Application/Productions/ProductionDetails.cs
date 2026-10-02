using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>
/// A production as its page shows it. <see cref="Version"/> is the production's, also the <c>ETag</c> (api §9);
/// <see cref="RiderVersion"/> is what saving a rider version sends.
/// </summary>
public sealed record ProductionDetails(
    ProductionId Id,
    Guid ArtistPartyId,
    string? ArtistName,
    bool IsArtistActive,
    string Name,
    string? Description,
    RiderId RiderId,
    int RiderVersion,
    int LatestVersionNumber,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
