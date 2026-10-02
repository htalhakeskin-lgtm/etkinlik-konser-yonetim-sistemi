namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>The body of <c>POST /api/v1/parties/{partyId}/contact-persons</c>.</summary>
public sealed record AddContactPersonRequest(Guid PersonId, string? Title = null);
