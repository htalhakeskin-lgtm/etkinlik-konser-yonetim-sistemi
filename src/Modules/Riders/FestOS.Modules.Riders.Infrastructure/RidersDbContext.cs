using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Riders.Domain;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;
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

    /// <summary>The riders' versions, only ever added.</summary>
    public DbSet<RiderVersion> RiderVersions => Set<RiderVersion>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_productions_artist_name"] = RidersRuleCodes.UniqueProductionName,
            ["ck_riders_source"] = RidersRuleCodes.RiderSource,
            ["ux_rider_versions_number"] = RidersRuleCodes.VersionNumbers,
            ["ck_rider_lines_target"] = RidersRuleCodes.LineTarget,
            ["ck_rider_lines_quantity"] = RidersRuleCodes.LineTarget,
            ["ck_rider_lines_flexibility"] = RidersRuleCodes.LineTarget,
            ["ux_rider_equivalent_models_model"] = RidersRuleCodes.EquivalentModels,
        };
}
