namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>The body of <c>PUT /api/v1/parties/{partyId}/representations/{representationId}</c>.</summary>
public sealed record RepresentationDescriptionRequest(string? Description = null);
