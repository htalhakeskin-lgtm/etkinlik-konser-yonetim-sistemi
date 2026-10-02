using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>An artist an agency represents; changed on the artist.</summary>
public sealed record RepresentedArtistItem(PartyId ArtistId, string Name, string? Description, bool IsActive);
