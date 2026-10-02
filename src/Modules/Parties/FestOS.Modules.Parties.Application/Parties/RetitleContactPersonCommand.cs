using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Changes a contact person's job title.</summary>
public sealed record RetitleContactPersonCommand(PartyId OrganizationId, OrganizationContactId ContactId, string? Title)
    : ICommand<bool>;
