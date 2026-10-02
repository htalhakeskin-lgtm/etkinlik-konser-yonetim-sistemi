using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Riders.Infrastructure;

internal sealed class RiderConfiguration : IEntityTypeConfiguration<Rider>
{
    public void Configure(EntityTypeBuilder<Rider> builder)
    {
        builder.ToTable("riders");

        // The production is another aggregate: no cascade, the conventions make it RESTRICT (database §10.2).
        // One rider per production; the system opens it, so a clash is no user's mistake (06 §5.6).
        builder.HasOne<Production>().WithMany().HasForeignKey(rider => rider.ProductionId);
        builder.HasIndex(rider => rider.ProductionId).IsUnique().HasDatabaseName("ux_riders_production_id");

        // A production's rider names its production (BR-RDR-007); a customer's joins with its events.
        builder.ToTable(table =>
            table.HasCheckConstraint("ck_riders_source", "source = 'production' AND production_id IS NOT NULL")
        );
    }
}
