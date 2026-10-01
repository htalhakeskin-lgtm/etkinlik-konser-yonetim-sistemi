using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

internal sealed class ListWarehousesHandler(InventoryDbContext context)
    : IQueryHandler<ListWarehousesQuery, PagedResult<WarehouseListItem>>
{
    public static SortKeys<Warehouse> SortKeys { get; } =
        new SortKeys<Warehouse>()
            .Add("name", warehouse => EF.Functions.Collate(warehouse.Name, Collations.Turkish))
            .Add("city", warehouse => EF.Functions.Collate(warehouse.City, Collations.Turkish));

    public Task<PagedResult<WarehouseListItem>> HandleAsync(
        ListWarehousesQuery query,
        CancellationToken cancellationToken
    )
    {
        IQueryable<Warehouse> warehouses = context.Warehouses.AsNoTracking();
        warehouses = query.Status switch
        {
            WarehouseStatusFilter.Active => warehouses.Where(warehouse => warehouse.DeactivatedAt == null),
            WarehouseStatusFilter.Inactive => warehouses.Where(warehouse => warehouse.DeactivatedAt != null),
            _ => warehouses,
        };
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            warehouses = warehouses.Where(warehouse => warehouse.NameSearch.Contains(key));
        }

        return SortKeys
            .Apply(warehouses, SortSpec.Parse(query.Sort, ListWarehousesQuery.DefaultSort), warehouse => warehouse.Id)
            .Select(warehouse => new WarehouseListItem(
                warehouse.Id,
                warehouse.Name,
                warehouse.City,
                warehouse.Address,
                warehouse.DeactivatedAt == null,
                warehouse.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);
    }
}
