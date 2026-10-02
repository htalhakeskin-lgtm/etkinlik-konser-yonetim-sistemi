using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Records that an agency represents an artist (US-ART-001, BR-PTY-004).</summary>
public sealed record AddRepresentationCommand(PartyId ArtistId, PartyId AgencyId, string? Description)
    : ICommand<ArtistRepresentationId>;
