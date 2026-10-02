using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Ends a representation.</summary>
public sealed record RemoveRepresentationCommand(PartyId ArtistId, ArtistRepresentationId RepresentationId)
    : ICommand<bool>;
