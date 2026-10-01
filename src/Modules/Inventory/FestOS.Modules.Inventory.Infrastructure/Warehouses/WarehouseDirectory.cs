using FestOS.Modules.Inventory.Contracts;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

/// <summary>Answers other modules from Inventory's own schema, with its own role (inventory §6).</summary>
internal sealed class WarehouseDirectory(InventoryDbContext context) : IWarehouseDirectory
{
    public async Task<IReadOnlySet<Guid>> FindActiveAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Count == 0)
        {
            return new HashSet<Guid>();
        }

        // Compared as identifiers, which EF translates; the identifier's value is read after the query.
        List<WarehouseId> wanted = [.. ids.Select(WarehouseId.From)];
        List<WarehouseId> found = await context
            .Warehouses.AsNoTracking()
            .Where(warehouse => warehouse.DeactivatedAt == null && wanted.Contains(warehouse.Id))
            .Select(warehouse => warehouse.Id)
            .ToListAsync(cancellationToken);
        return found.Select(id => id.Value).ToHashSet();
    }
}
