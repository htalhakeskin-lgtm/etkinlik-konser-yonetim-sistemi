using FestOS.Modules.Sample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Sample.Infrastructure;

internal sealed class SampleItemConfiguration : IEntityTypeConfiguration<SampleItem>
{
    public void Configure(EntityTypeBuilder<SampleItem> builder)
    {
        builder.ToTable("sample_items");
        builder.Property(item => item.Name).HasMaxLength(200);

        // Parts belong to the aggregate, so this cascade is explicit (V-11).
        builder
            .HasMany(item => item.Parts)
            .WithOne()
            .HasForeignKey("SampleItemId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(item => item.Parts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
