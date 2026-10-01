using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>The warehouses list (US-SYS-005): searched by name, filtered by status, one numbered page at a time.</summary>
public sealed record ListWarehousesQuery(string? Q, WarehouseStatusFilter Status, string? Sort, PageRequest Page)
    : IQuery<PagedResult<WarehouseListItem>>
{
    /// <summary>The fields <see cref="Sort"/> may name.</summary>
    public static IReadOnlyList<string> SortFields { get; } = ["name", "city"];

    /// <summary>The order without a <see cref="Sort"/>.</summary>
    public const string DefaultSort = "name";

    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
