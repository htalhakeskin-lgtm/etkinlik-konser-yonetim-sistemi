using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>A party as its detail page shows it; <see cref="Version"/> is also the <c>ETag</c> (api §9).</summary>
public sealed record PartyDetails(
    PartyId Id,
    PartyKind Kind,
    string Name,
    string? FirstName,
    string? LastName,
    string? LegalName,
    IReadOnlyList<PartyRole> Roles,
    IReadOnlyList<ContactPointItem> ContactPoints,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
