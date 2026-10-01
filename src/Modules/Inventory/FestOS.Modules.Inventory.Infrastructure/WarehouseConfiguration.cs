using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Inventory.Infrastructure;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.Property(warehouse => warehouse.Name).HasMaxLength(Warehouse.NameMaxLength);
        builder.Property(warehouse => warehouse.City).HasMaxLength(Warehouse.CityMaxLength);
        builder.Property(warehouse => warehouse.Address).HasMaxLength(Warehouse.AddressMaxLength);

        // Names are compared by their search key, so letter case and Turkish marks make no second name;
        // deactivated warehouses keep theirs (BR-SYS-016, database §13).
        builder.Property(warehouse => warehouse.NameSearch).HasMaxLength(Warehouse.NameMaxLength);
        builder.HasIndex(warehouse => warehouse.NameSearch).IsUnique().HasDatabaseName("ux_warehouses_name");
    }
}
