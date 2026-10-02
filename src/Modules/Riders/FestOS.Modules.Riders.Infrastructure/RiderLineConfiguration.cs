using FestOS.Modules.Riders.Domain.RiderVersions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Riders.Infrastructure;

internal sealed class RiderLineConfiguration : IEntityTypeConfiguration<RiderLine>
{
    public void Configure(EntityTypeBuilder<RiderLine> builder)
    {
        builder.ToTable("rider_lines");

        // The targets are other modules' records: indexed by hand (database §12.2, DT-03).
        builder.HasIndex(line => line.ModelId);
        builder.HasIndex(line => line.CategoryId);
        builder.HasIndex(line => line.KitId);

        builder.ToTable(table =>
        {
            // One target, at least one of it, flexibility on model lines only (BR-RDR-001, riders RD-02).
            table.HasCheckConstraint("ck_rider_lines_target", "num_nonnulls(model_id, category_id, kit_id) = 1");
            table.HasCheckConstraint("ck_rider_lines_quantity", "quantity >= 1");
            table.HasCheckConstraint("ck_rider_lines_flexibility", "(model_id IS NULL) = (flexibility IS NULL)");
        });

        // The equivalents belong to the line, so this cascade is explicit (database §10.2).
        builder
            .HasMany(line => line.Equivalents)
            .WithOne()
            .HasForeignKey("RiderLineId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(line => line.Equivalents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
