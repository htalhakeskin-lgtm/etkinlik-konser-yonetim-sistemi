using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Venues.Infrastructure;

internal sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");
        builder.Property(venue => venue.Name).HasMaxLength(Venue.NameMaxLength);
        builder.Property(venue => venue.NameSearch).HasMaxLength(Venue.NameMaxLength);
        builder.Property(venue => venue.City).HasMaxLength(Venue.CityMaxLength);
        builder.Property(venue => venue.CitySearch).HasMaxLength(Venue.CityMaxLength);
        builder.Property(venue => venue.Address).HasMaxLength(Venue.AddressMaxLength);
        builder.Property(venue => venue.LoadingDock).HasMaxLength(Venue.LoadingDockMaxLength);
        builder.Property(venue => venue.TimeZone).HasMaxLength(Venue.TimeZoneMaxLength);

        // Physical values carry their unit in the name (database §8.3).
        builder.Property(venue => venue.StageWidthMeters).HasPrecision(6, 2);
        builder.Property(venue => venue.StageDepthMeters).HasPrecision(6, 2);
        builder.Property(venue => venue.StageHeightMeters).HasPrecision(6, 2);
        builder.Property(venue => venue.PowerCapacityAmperes).HasPrecision(7, 2);

        // One venue per name in a city, deactivated ones included (BR-VEN-003, venues VN-02).
        builder
            .HasIndex(venue => new { venue.CitySearch, venue.NameSearch })
            .IsUnique()
            .HasDatabaseName("ux_venues_name_city");

        // The operator is a party, another module's record: indexed by hand (database §12.2, DT-03).
        builder.HasIndex(venue => venue.OperatorPartyId);

        // The API checks these too; the constraints repeat a field check (parties MD-05).
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_venues_capacity", "capacity > 0");
            table.HasCheckConstraint("ck_venues_stage_width_meters", "stage_width_meters > 0");
            table.HasCheckConstraint("ck_venues_stage_depth_meters", "stage_depth_meters > 0");
            table.HasCheckConstraint("ck_venues_stage_height_meters", "stage_height_meters > 0");
            table.HasCheckConstraint("ck_venues_power_capacity_amperes", "power_capacity_amperes > 0");
        });
    }
}
