using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>
/// The venues list (US-VEN-001): searched by name and city, filtered by city and status, one numbered page
/// at a time; each row names its operator (parties MD-02).
/// </summary>
public sealed record ListVenuesQuery(string? Q, string? City, VenueStatusFilter Status, string? Sort, PageRequest Page)
    : IQuery<PagedResult<VenueListItem>>
{
    /// <summary>The fields <see cref="Sort"/> may name.</summary>
    public static IReadOnlyList<string> SortFields { get; } = ["name", "city", "capacity"];

    /// <summary>The order without a <see cref="Sort"/>.</summary>
    public const string DefaultSort = "name";

    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
