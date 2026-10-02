namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>The body of <c>POST /api/v1/parties/{partyId}/representations</c>.</summary>
public sealed record AddRepresentationRequest(Guid AgencyId, string? Description = null);
