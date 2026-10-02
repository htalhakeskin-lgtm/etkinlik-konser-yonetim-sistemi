using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Catalog.Domain;
using FestOS.Modules.Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure;

/// <summary>The Catalog module's context, on the <c>catalog</c> schema.</summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : ModuleDbContext(options, CatalogModuleDefinition.SchemaName)
{
    /// <summary>The equipment categories.</summary>
    public DbSet<EquipmentCategory> Categories => Set<EquipmentCategory>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_equipment_categories_name"] = CatalogRuleCodes.UniqueCategoryName,
        };
}
