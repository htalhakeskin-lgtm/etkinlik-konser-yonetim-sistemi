using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Venues.Infrastructure;

internal sealed class VenueEquipmentUnavailabilityConfiguration : IEntityTypeConfiguration<VenueEquipmentUnavailability>
{
    public void Configure(EntityTypeBuilder<VenueEquipmentUnavailability> builder)
    {
        builder.ToTable("venue_equipment_unavailabilities");
        builder.Property(period => period.Reason).HasMaxLength(VenueEquipmentUnavailability.ReasonMaxLength);

        // The periods of a line do not overlap: an exclusion constraint the migration writes in SQL
        // (ex_venue_equipment_unavailabilities_overlap, database §12.1, BR-VEN-002).
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_venue_equipment_unavailabilities_period", "period_start < period_end");

            // The API checks this too (parties MD-05).
            table.HasCheckConstraint("ck_venue_equipment_unavailabilities_quantity", "quantity >= 1");
        });
    }
}
