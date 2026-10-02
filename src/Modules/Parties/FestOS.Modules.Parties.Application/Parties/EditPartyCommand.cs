using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Changes a party's names, roles and contact points; its kind stays (US-PTY-002, BR-PTY-001).</summary>
public sealed record EditPartyCommand(
    PartyId Id,
    string Name,
    string? FirstName,
    string? LastName,
    string? LegalName,
    IReadOnlyList<PartyRole> Roles,
    IReadOnlyList<ContactPointDetails> ContactPoints
) : ICommand<bool>, IPartyDescription;
