using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>
/// The productions list (US-ART-001): one artist's or all, searched by name, filtered by status, one
/// numbered page at a time; each row names its artist (parties MD-02).
/// </summary>
public sealed record ListProductionsQuery(
    Guid? ArtistId,
    string? Q,
    ProductionStatusFilter Status,
    string? Sort,
    PageRequest Page
) : IQuery<PagedResult<ProductionListItem>>
{
    /// <summary>The fields <see cref="Sort"/> may name.</summary>
    public static IReadOnlyList<string> SortFields { get; } = ["name", "createdAt"];

    /// <summary>The order without a <see cref="Sort"/>.</summary>
    public const string DefaultSort = "name";

    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
