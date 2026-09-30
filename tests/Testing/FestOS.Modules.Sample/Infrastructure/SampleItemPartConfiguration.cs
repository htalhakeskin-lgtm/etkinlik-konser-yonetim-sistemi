using FestOS.Modules.Sample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Sample.Infrastructure;

internal sealed class SampleItemPartConfiguration : IEntityTypeConfiguration<SampleItemPart>
{
    public void Configure(EntityTypeBuilder<SampleItemPart> builder)
    {
        builder.ToTable("sample_item_parts");
        builder.Property(part => part.Label).HasMaxLength(200);

        // No delete behaviour here: the conventions make it RESTRICT.
        builder.HasOne<SampleItemPart>().WithMany().HasForeignKey(part => part.ReplacesPartId);
    }
}
