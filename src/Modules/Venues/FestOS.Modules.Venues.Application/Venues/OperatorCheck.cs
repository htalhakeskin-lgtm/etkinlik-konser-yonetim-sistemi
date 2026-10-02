using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Parties.Contracts;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>
/// A venue's operator is an active party with the venue operator role (BR-PTY-004), asked of Parties
/// synchronously (05 §4.3, venues §5).
/// </summary>
internal static class OperatorCheck
{
    public static async Task EnsureSelectableOperatorAsync(
        this IPartyDirectory parties,
        Guid? operatorPartyId,
        CancellationToken cancellationToken
    )
    {
        if (operatorPartyId is not { } id)
        {
            return;
        }

        IReadOnlyDictionary<Guid, PartySummary> found = await parties.FindAsync([id], cancellationToken);
        if (!found.TryGetValue(id, out PartySummary? party) || !party.IsSelectableAs(PartyRoles.VenueOperator))
        {
            throw new BusinessRuleViolationException(
                PartyRoles.SelectionRuleCode,
                "A venue's operator is an active party with the venue operator role.",
                parameters: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["field"] = "operatorPartyId",
                    ["role"] = PartyRoles.VenueOperator,
                }
            );
        }
    }
}
