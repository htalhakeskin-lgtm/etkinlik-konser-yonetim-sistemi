using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Opens a deactivated party again.</summary>
public sealed record ActivatePartyCommand(PartyId Id) : ICommand<bool>;
