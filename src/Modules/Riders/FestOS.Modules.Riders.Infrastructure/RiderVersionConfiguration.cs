using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Riders.Infrastructure;

internal sealed class RiderVersionConfiguration : IEntityTypeConfiguration<RiderVersion>
{
    public void Configure(EntityTypeBuilder<RiderVersion> builder)
    {
        builder.ToTable("rider_versions");
        builder.Property(version => version.Note).HasMaxLength(RiderVersion.NoteMaxLength);
        builder.Property(version => version.CreatedByName).HasMaxLength(RiderVersion.CreatedByNameMaxLength);

        // The rider is another aggregate: no cascade, the conventions make it RESTRICT (database §10.2).
        // Versions are numbered one after another within a rider (BR-RDR-003).
        builder.HasOne<Rider>().WithMany().HasForeignKey(version => version.RiderId);
        builder
            .HasIndex(version => new { version.RiderId, version.Number })
            .IsUnique()
            .HasDatabaseName("ux_rider_versions_number");

        // The lines belong to the version, so this cascade is explicit (database §10.2).
        builder
            .HasMany(version => version.Lines)
            .WithOne()
            .HasForeignKey("RiderVersionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(version => version.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
