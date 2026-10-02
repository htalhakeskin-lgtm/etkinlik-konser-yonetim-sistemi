using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>
/// The models list (US-EQP-002): searched by brand and name, filtered by category (its subtree included),
/// tracking type and status, one numbered page at a time. Pickers call it with <c>status=active</c>.
/// </summary>
public sealed record ListEquipmentModelsQuery(
    string? Q,
    EquipmentCategoryId? CategoryId,
    TrackingType? TrackingType,
    ModelStatusFilter Status,
    string? Sort,
    PageRequest Page
) : IQuery<PagedResult<EquipmentModelListItem>>
{
    /// <summary>The fields <see cref="Sort"/> may name.</summary>
    public static IReadOnlyList<string> SortFields { get; } = ["name", "brand"];

    /// <summary>The order without a <see cref="Sort"/>.</summary>
    public const string DefaultSort = "name";

    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
