using FestOS.Modules.Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Catalog.Infrastructure;

internal sealed class EquipmentCategoryConfiguration : IEntityTypeConfiguration<EquipmentCategory>
{
    public void Configure(EntityTypeBuilder<EquipmentCategory> builder)
    {
        builder.ToTable("equipment_categories");
        builder.Property(category => category.Name).HasMaxLength(EquipmentCategory.NameMaxLength);
        builder.Property(category => category.NameSearch).HasMaxLength(EquipmentCategory.NameMaxLength);

        // The parent is another category: no cascade, the conventions make it RESTRICT (database §10.2).
        builder.HasOne<EquipmentCategory>().WithMany().HasForeignKey(category => category.ParentId);

        // One name per parent, the top level counting as one parent; deactivated ones keep theirs (BR-EQP-011).
        builder
            .HasIndex(category => new { category.ParentId, category.NameSearch })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName("ux_equipment_categories_name");
    }
}
