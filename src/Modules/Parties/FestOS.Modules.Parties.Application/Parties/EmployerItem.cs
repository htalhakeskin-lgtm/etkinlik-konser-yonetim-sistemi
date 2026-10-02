using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>An organization a person is a contact of; changed on the organization.</summary>
public sealed record EmployerItem(PartyId OrganizationId, string Name, string? Title, bool IsActive);
