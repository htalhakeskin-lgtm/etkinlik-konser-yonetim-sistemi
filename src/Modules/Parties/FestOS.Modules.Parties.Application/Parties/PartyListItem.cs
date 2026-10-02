using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>A row of the parties list; <c>Version</c> lets the row's actions send <c>If-Match</c> (api §9).</summary>
public sealed record PartyListItem(
    PartyId Id,
    PartyKind Kind,
    string Name,
    IReadOnlyList<PartyRole> Roles,
    string? PrimaryPhone,
    string? PrimaryEmail,
    bool IsActive,
    int Version
);
