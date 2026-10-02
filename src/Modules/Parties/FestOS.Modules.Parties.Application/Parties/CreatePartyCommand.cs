using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Records a new person or organization with its roles and contact points (US-PTY-001).</summary>
public sealed record CreatePartyCommand(
    PartyKind Kind,
    string Name,
    string? FirstName,
    string? LastName,
    string? LegalName,
    IReadOnlyList<PartyRole> Roles,
    IReadOnlyList<ContactPointDetails> ContactPoints
) : ICommand<PartyId>, IPartyDescription;
