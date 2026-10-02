using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Changes what a representation covers.</summary>
public sealed record DescribeRepresentationCommand(
    PartyId ArtistId,
    ArtistRepresentationId RepresentationId,
    string? Description
) : ICommand<bool>;
