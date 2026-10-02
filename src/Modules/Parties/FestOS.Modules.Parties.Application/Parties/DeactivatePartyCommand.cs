using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Takes a party out of new selections (US-PTY-002, BR-SYS-001).</summary>
public sealed record DeactivatePartyCommand(PartyId Id) : ICommand<bool>;
