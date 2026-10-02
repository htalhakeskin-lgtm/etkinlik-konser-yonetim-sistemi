using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>
/// The parties list (US-PTY-002): searched by name and contact point, filtered by role, kind and status,
/// one numbered page at a time. Pickers call it with <c>status=active</c> (parties MD-03).
/// </summary>
public sealed record ListPartiesQuery(
    string? Q,
    PartyRole? Role,
    PartyKind? Kind,
    PartyStatusFilter Status,
    string? Sort,
    PageRequest Page
) : IQuery<PagedResult<PartyListItem>>
{
    /// <summary>The fields <see cref="Sort"/> may name.</summary>
    public static IReadOnlyList<string> SortFields { get; } = ["name", "createdAt"];

    /// <summary>The order without a <see cref="Sort"/>.</summary>
    public const string DefaultSort = "name";

    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
