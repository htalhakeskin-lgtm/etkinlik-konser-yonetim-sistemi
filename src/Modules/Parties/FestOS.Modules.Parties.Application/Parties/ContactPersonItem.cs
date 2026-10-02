using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>A contact person of an organization, with the person's name and primary phone and e-mail.</summary>
public sealed record ContactPersonItem(
    OrganizationContactId Id,
    PartyId PersonId,
    string Name,
    string? Title,
    string? PrimaryPhone,
    string? PrimaryEmail,
    bool IsActive
);
