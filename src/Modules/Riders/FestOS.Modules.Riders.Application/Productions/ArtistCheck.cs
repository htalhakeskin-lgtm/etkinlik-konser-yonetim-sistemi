using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Parties.Contracts;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>
/// A production's artist is an active party with the artist role (BR-PTY-004), asked of Parties
/// synchronously (05 §4.3, riders §5).
/// </summary>
internal static class ArtistCheck
{
    public static async Task EnsureSelectableArtistAsync(
        this IPartyDirectory parties,
        Guid artistPartyId,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<Guid, PartySummary> found = await parties.FindAsync([artistPartyId], cancellationToken);
        if (!found.TryGetValue(artistPartyId, out PartySummary? party) || !party.IsSelectableAs(PartyRoles.Artist))
        {
            throw new BusinessRuleViolationException(
                PartyRoles.SelectionRuleCode,
                "A production's artist is an active party with the artist role.",
                parameters: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["field"] = "artistPartyId",
                    ["role"] = PartyRoles.Artist,
                }
            );
        }
    }
}
