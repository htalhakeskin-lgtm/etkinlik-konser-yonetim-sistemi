using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Catalog.Application.Kits;
using FestOS.Modules.Catalog.Domain.Kits;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

internal sealed class ListKitsHandler(CatalogDbContext context) : IQueryHandler<ListKitsQuery, PagedResult<KitListItem>>
{
    public async Task<PagedResult<KitListItem>> HandleAsync(ListKitsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Kit> kits = context.Kits.AsNoTracking();
        kits = query.Status switch
        {
            KitStatusFilter.Active => kits.Where(kit => kit.DeactivatedAt == null),
            KitStatusFilter.Inactive => kits.Where(kit => kit.DeactivatedAt != null),
            _ => kits,
        };
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            kits = kits.Where(kit => kit.NameSearch.Contains(key));
        }

        PagedResult<KitListItem> page = await kits.OrderBy(kit => EF.Functions.Collate(kit.Name, Collations.Turkish))
            .ThenBy(kit => kit.Id)
            .Select(kit => new KitListItem(
                kit.Id,
                kit.Name,
                kit.Lines.Count,
                new KitTotals(0m, true, 0, true),
                kit.DeactivatedAt == null,
                kit.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);
        KitCatalog catalog = await KitCatalog.ReadAsync(context, cancellationToken);
        return page with { Items = [.. page.Items.Select(item => item with { Totals = catalog.TotalsOf(item.Id) })] };
    }
}
