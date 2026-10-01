using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Application.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

internal sealed class GetWarehouseHandler(InventoryDbContext context)
    : IQueryHandler<GetWarehouseQuery, WarehouseDetails>
{
    public async Task<WarehouseDetails> HandleAsync(GetWarehouseQuery query, CancellationToken cancellationToken) =>
        await context
            .Warehouses.AsNoTracking()
            .Where(warehouse => warehouse.Id == query.Id)
            .Select(warehouse => new WarehouseDetails(
                warehouse.Id,
                warehouse.Name,
                warehouse.City,
                warehouse.Address,
                warehouse.DeactivatedAt,
                warehouse.CreatedAt,
                warehouse.UpdatedAt,
                warehouse.Version
            ))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException("Warehouse", query.Id.Value);
}
