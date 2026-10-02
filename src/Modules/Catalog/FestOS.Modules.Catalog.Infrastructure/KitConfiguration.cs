using FestOS.Modules.Catalog.Domain.Kits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Catalog.Infrastructure;

internal sealed class KitConfiguration : IEntityTypeConfiguration<Kit>
{
    public void Configure(EntityTypeBuilder<Kit> builder)
    {
        builder.ToTable("kits");
        builder.Property(kit => kit.Name).HasMaxLength(Kit.NameMaxLength);
        builder.Property(kit => kit.NameSearch).HasMaxLength(Kit.NameMaxLength);

        // One kit per name, deactivated ones included (BR-EQP-013).
        builder.HasIndex(kit => kit.NameSearch).IsUnique().HasDatabaseName("ux_kits_name");

        // The lines belong to the kit, so this cascade is explicit (database §10.2).
        builder
            .HasMany(kit => kit.Lines)
            .WithOne()
            .HasForeignKey("KitId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(kit => kit.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
