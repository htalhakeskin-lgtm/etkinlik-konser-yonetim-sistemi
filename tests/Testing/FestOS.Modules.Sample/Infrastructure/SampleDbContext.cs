using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Sample.Domain;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Sample.Infrastructure;

/// <summary>The sample module's database context.</summary>
public sealed class SampleDbContext(DbContextOptions<SampleDbContext> options)
    : ModuleDbContext(options, SampleModuleDefinition.SchemaName)
{
    /// <summary>The items.</summary>
    public DbSet<SampleItem> SampleItems => Set<SampleItem>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_sample_items_name"] = SampleRuleCodes.NameIsUnique,
        };
}
