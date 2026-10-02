using System.Text.Json;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>Answers other modules from the Parties schema, with its own role (05 §2, principle 1).</summary>
internal sealed class PartyDirectory(PartiesDbContext context) : IPartyDirectory
{
    public async Task<IReadOnlyDictionary<Guid, PartySummary>> FindAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        // EF compares the typed identifiers, not their Value (inventory §6).
        List<PartyId> partyIds = [.. ids.Distinct().Select(PartyId.From)];
        var parties = await context
            .Parties.AsNoTracking()
            .Where(party => partyIds.Contains(party.Id))
            .Select(party => new
            {
                party.Id,
                party.Name,
                party.DeactivatedAt,
                party.Roles,
            })
            .ToListAsync(cancellationToken);
        return parties.ToDictionary(
            party => party.Id.Value,
            party => new PartySummary(
                party.Id.Value,
                party.Name,
                party.DeactivatedAt is null,
                [.. party.Roles.Select(RoleName)]
            )
        );
    }

    // The API's text for the role: camelCase, as the database stores it (database §6.2).
    private static string RoleName(PartyRole role) => JsonNamingPolicy.CamelCase.ConvertName(role.ToString());
}
