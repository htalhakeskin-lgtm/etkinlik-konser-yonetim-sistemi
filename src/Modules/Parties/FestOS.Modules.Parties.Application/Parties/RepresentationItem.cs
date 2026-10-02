using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>An agency that represents an artist.</summary>
public sealed record RepresentationItem(
    ArtistRepresentationId Id,
    PartyId AgencyId,
    string AgencyName,
    string? Description,
    bool IsActive
);
