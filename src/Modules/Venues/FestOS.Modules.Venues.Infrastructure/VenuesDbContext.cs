using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Venues.Domain;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Venues.Infrastructure;

/// <summary>The Venues module's context, on the <c>venues</c> schema.</summary>
public sealed class VenuesDbContext(DbContextOptions<VenuesDbContext> options)
    : ModuleDbContext(options, VenuesModuleDefinition.SchemaName)
{
    /// <summary>The venues.</summary>
    public DbSet<Venue> Venues => Set<Venue>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_venues_name_city"] = VenuesRuleCodes.UniqueVenueName,
            ["ck_venue_equipment_target"] = VenuesRuleCodes.EquipmentTarget,
            ["ck_venue_equipment_validity"] = VenuesRuleCodes.EquipmentPeriods,
            ["ck_venue_equipment_unavailabilities_period"] = VenuesRuleCodes.EquipmentPeriods,
            ["ex_venue_equipment_unavailabilities_overlap"] = VenuesRuleCodes.EquipmentPeriods,
        };
}
