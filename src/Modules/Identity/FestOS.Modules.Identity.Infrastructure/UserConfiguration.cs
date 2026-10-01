using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Identity.Infrastructure;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.FullNameSearch).HasMaxLength(User.FullNameMaxLength);
        // Searching inside a name ("contains") needs trigrams; a b-tree only serves prefixes (database §13).
        builder
            .HasIndex(user => user.FullNameSearch)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("ix_users_full_name_search");
        builder.ToTable("users");
        builder.Ignore(user => user.NeedsWarehouse);
        builder.Property(user => user.FullName).HasMaxLength(User.FullNameMaxLength);
        builder.Property(user => user.Email).HasMaxLength(EmailAddress.MaxLength);
        builder.HasIndex(user => user.Email).IsUnique().HasDatabaseName("ux_users_email");

        // Roles and assigned warehouses are arrays on the user (ID-10); GIN indexes answer "who has it".
        builder
            .PrimitiveCollection(user => user.Roles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .ElementType(role => role.HasConversion<CamelCaseEnumConverter<Role>>());
        builder.HasIndex(user => user.Roles).HasMethod("gin").HasDatabaseName("ix_users_roles");
        builder.ToTable(table =>
            table.HasCheckConstraint(
                "ck_users_roles_enum",
                $"roles <@ ARRAY[{string.Join(", ", CamelCaseEnumConverter<Role>.StoredValues.Select(value => $"'{value}'"))}]::text[]"
            )
        );
        builder
            .PrimitiveCollection(user => user.WarehouseIds)
            .HasField("_warehouseIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(user => user.WarehouseIds).HasMethod("gin").HasDatabaseName("ix_users_warehouse_ids");
    }
}
