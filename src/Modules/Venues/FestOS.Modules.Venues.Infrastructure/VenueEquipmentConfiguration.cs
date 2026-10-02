using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Venues.Infrastructure;

internal sealed class VenueEquipmentConfiguration : IEntityTypeConfiguration<VenueEquipment>
{
    public void Configure(EntityTypeBuilder<VenueEquipment> builder)
    {
        builder.ToTable("venue_equipment");
        builder.Ignore(line => line.IsCounted);
        builder.Property(line => line.Description).HasMaxLength(VenueEquipment.DescriptionMaxLength);

        // Models and categories are Catalog's records: indexed by hand (database §12.2, DT-03).
        builder.HasIndex(line => line.ModelId);
        builder.HasIndex(line => line.CategoryId);

        builder
            .HasMany(line => line.Unavailabilities)
            .WithOne()
            .HasForeignKey("VenueEquipmentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(line => line.Unavailabilities).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable(table =>
        {
            // A line asks for exactly one of a model, a category or a free description (06 decision 3, BR-VEN-001).
            table.HasCheckConstraint(
                "ck_venue_equipment_target",
                "num_nonnulls(model_id, category_id, description) = 1"
            );

            // The validity ends after it starts; an open end is unbounded (BR-VEN-002).
            table.HasCheckConstraint(
                "ck_venue_equipment_validity",
                "validity_start IS NULL OR validity_end IS NULL OR validity_start < validity_end"
            );

            // The API checks this too (parties MD-05).
            table.HasCheckConstraint("ck_venue_equipment_quantity", "quantity >= 1");
        });
    }
}
