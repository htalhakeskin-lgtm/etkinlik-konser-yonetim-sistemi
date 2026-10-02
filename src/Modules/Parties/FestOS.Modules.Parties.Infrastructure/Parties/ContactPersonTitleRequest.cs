namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>The body of <c>PUT /api/v1/parties/{partyId}/contact-persons/{contactId}</c>.</summary>
public sealed record ContactPersonTitleRequest(string? Title = null);
