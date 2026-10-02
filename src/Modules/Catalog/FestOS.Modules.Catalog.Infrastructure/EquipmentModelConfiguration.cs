using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Catalog.Infrastructure;

internal sealed class EquipmentModelConfiguration : IEntityTypeConfiguration<EquipmentModel>
{
    public void Configure(EntityTypeBuilder<EquipmentModel> builder)
    {
        builder.ToTable("equipment_models");
        builder.Ignore(model => model.DisplayName);
        builder.Property(model => model.Brand).HasMaxLength(EquipmentModel.BrandMaxLength);
        builder.Property(model => model.Name).HasMaxLength(EquipmentModel.NameMaxLength);
        builder
            .Property(model => model.BrandNameSearch)
            .HasMaxLength(EquipmentModel.BrandMaxLength + EquipmentModel.NameMaxLength + 1);

        // Physical values carry their unit in the name (database §8.3).
        builder.Property(model => model.WeightKilograms).HasPrecision(10, 3);
        builder.Property(model => model.TransportVolumeCubicMeters).HasPrecision(8, 3);

        // The category is another aggregate: no cascade, the conventions make it RESTRICT (database §10.2).
        builder.HasOne<EquipmentCategory>().WithMany().HasForeignKey(model => model.CategoryId);

        // One model per brand and name, deactivated ones included (BR-EQP-012); trigrams serve the search.
        // Two indexes on one column need their own names, or EF folds them into one.
        builder
            .HasIndex(model => model.BrandNameSearch, "ux_equipment_models_brand_name")
            .IsUnique()
            .HasDatabaseName("ux_equipment_models_brand_name");
        builder
            .HasIndex(model => model.BrandNameSearch, "ix_equipment_models_search")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("ix_equipment_models_search");

        // The API checks these too; the constraints repeat a field check (parties MD-05).
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_equipment_models_weight_kilograms", "weight_kilograms > 0");
            table.HasCheckConstraint("ck_equipment_models_power_watts", "power_watts > 0");
            table.HasCheckConstraint(
                "ck_equipment_models_transport_volume_cubic_meters",
                "transport_volume_cubic_meters > 0"
            );
        });
    }
}
