using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>A row of the productions list; <c>Version</c> lets the row's actions send <c>If-Match</c> (api §9).</summary>
public sealed record ProductionListItem(
    ProductionId Id,
    Guid ArtistPartyId,
    string? ArtistName,
    string Name,
    int LatestVersionNumber,
    bool IsActive,
    int Version
);
