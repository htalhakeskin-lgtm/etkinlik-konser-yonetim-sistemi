using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Ties a person to an organization as its contact person (US-PTY-001, BR-PTY-003).</summary>
public sealed record AddContactPersonCommand(PartyId OrganizationId, PartyId PersonId, string? Title)
    : ICommand<OrganizationContactId>;
