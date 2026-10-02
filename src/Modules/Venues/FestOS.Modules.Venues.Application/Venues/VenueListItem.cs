using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>A row of the venues list; <c>Version</c> lets the row's actions send <c>If-Match</c> (api §9).</summary>
public sealed record VenueListItem(
    VenueId Id,
    string Name,
    string City,
    int Capacity,
    Guid? OperatorPartyId,
    string? OperatorName,
    bool IsActive,
    int Version
);
