using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>The kits list (US-EQP-005): searched by name, filtered by status, with each kit's totals.</summary>
public sealed record ListKitsQuery(string? Q, KitStatusFilter Status, PageRequest Page)
    : IQuery<PagedResult<KitListItem>>
{
    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
