using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Loads the party a command changes, at the version the request saw (api §9).</summary>
internal static class PartyLoader
{
    public static async Task<Party> LoadForChangeAsync(
        this IPartyRepository parties,
        PartyId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        Party party = await parties.FindAsync(id, cancellationToken) ?? throw new NotFoundException("Party", id.Value);
        expectedVersion.EnsureMatches(party);
        return party;
    }
}
