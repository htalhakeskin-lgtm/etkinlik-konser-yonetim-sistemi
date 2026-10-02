using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>One party with its contact points, for the detail page and the edit dialog.</summary>
public sealed record GetPartyQuery(PartyId Id) : IQuery<PartyDetails>;
