namespace FestOS.Modules.Riders.Infrastructure.Productions;

/// <summary>The body of <c>POST /api/v1/productions</c>; the description may be left out.</summary>
public sealed record CreateProductionRequest(Guid ArtistPartyId, string Name, string? Description = null);
