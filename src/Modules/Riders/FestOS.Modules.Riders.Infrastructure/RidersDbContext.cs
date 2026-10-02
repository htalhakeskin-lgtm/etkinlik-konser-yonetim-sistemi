using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Riders.Domain;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure;

/// <summary>The Riders module's context, on the <c>riders</c> schema.</summary>
public sealed class RidersDbContext(DbContextOptions<RidersDbContext> options)
    : ModuleDbContext(options, RidersModuleDefinition.SchemaName)
{
    /// <summary>The productions.</summary>
    public DbSet<Production> Productions => Set<Production>();

    /// <summary>The riders.</summary>
    public DbSet<Rider> Riders => Set<Rider>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_productions_artist_name"] = RidersRuleCodes.UniqueProductionName,
            ["ck_riders_source"] = RidersRuleCodes.RiderSource,
        };
}
