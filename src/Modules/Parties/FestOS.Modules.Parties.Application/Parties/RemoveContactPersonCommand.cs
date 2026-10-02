using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Unties a contact person from an organization; the person stays.</summary>
public sealed record RemoveContactPersonCommand(PartyId OrganizationId, OrganizationContactId ContactId)
    : ICommand<bool>;
