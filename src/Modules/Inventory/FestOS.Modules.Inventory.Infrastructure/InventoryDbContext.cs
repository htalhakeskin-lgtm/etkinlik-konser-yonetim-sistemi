using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Inventory.Domain;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Inventory.Infrastructure;

/// <summary>The Inventory module's context, on the <c>inventory</c> schema.</summary>
public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : ModuleDbContext(options, InventoryModuleDefinition.SchemaName)
{
    /// <summary>The warehouses.</summary>
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_warehouses_name"] = InventoryRuleCodes.UniqueWarehouseName,
        };
}
