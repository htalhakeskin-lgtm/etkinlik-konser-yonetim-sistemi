using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Catalog.Infrastructure;

internal sealed class KitLineConfiguration : IEntityTypeConfiguration<KitLine>
{
    public void Configure(EntityTypeBuilder<KitLine> builder)
    {
        builder.ToTable("kit_lines");

        // The model and the sub-kit are other aggregates: no cascade, the conventions make it RESTRICT.
        builder.HasOne<EquipmentModel>().WithMany().HasForeignKey(line => line.ModelId);
        builder.HasOne<Kit>().WithMany().HasForeignKey(line => line.SubKitId);

        builder.ToTable(table =>
        {
            // A line is a model or a kit, never both (06 decision 3, BR-EQP-003).
            table.HasCheckConstraint("ck_kit_lines_target", "num_nonnulls(model_id, sub_kit_id) = 1");

            // The API checks this too (parties MD-05).
            table.HasCheckConstraint("ck_kit_lines_quantity", "quantity >= 1");
        });
    }
}
