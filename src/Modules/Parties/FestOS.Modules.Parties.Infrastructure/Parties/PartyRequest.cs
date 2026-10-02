using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>
/// The body of <c>POST /api/v1/parties</c> and <c>PUT /api/v1/parties/{partyId}</c>. <see cref="Kind"/> is
/// read only on create; a party keeps its kind (BR-PTY-001). The names a kind does not use may be left out.
/// </summary>
public sealed record PartyRequest(
    string Name,
    IReadOnlyList<PartyRole> Roles,
    IReadOnlyList<ContactPointRequest> ContactPoints,
    PartyKind? Kind = null,
    string? FirstName = null,
    string? LastName = null,
    string? LegalName = null
)
{
    /// <summary>The contact points as the party takes them.</summary>
    public IReadOnlyList<ContactPointDetails> ContactPointDetails() =>
        [
            .. ContactPoints.Select(point => new ContactPointDetails(
                point.Id is { } id ? ContactPointId.From(id) : null,
                point.Kind,
                point.Value,
                point.Label,
                point.IsPrimary
            )),
        ];
}
