using FestOS.BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The model rules every module shares, applied after the module's own mappings
/// (building-blocks §5.3).
/// </summary>
internal static class ModelConventions
{
    /// <summary>Enum columns are short codes; values stay well under this length.</summary>
    public const int EnumMaxLength = 64;

    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Identifiers are chosen by the domain before saving (database §5.1). Integer keys belong only
            // to framework types such as ASP.NET's data protection keys; the database numbers those.
            foreach (IMutableProperty key in entityType.FindPrimaryKey()?.Properties ?? [])
            {
                if (key.ClrType != typeof(int) && key.ClrType != typeof(long))
                {
                    key.ValueGenerated = ValueGenerated.Never;
                }
            }

            // The aggregate's version guards against lost updates (V-09).
            if (IsAggregateRoot(entityType.ClrType))
            {
                entityType.FindProperty(nameof(AggregateRoot<>.Version))!.IsConcurrencyToken = true;
            }

            // Deleting a row never deletes other rows unless a mapping says so (V-11).
            foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
            {
                if (
                    ((IConventionForeignKey)foreignKey).GetDeleteBehaviorConfigurationSource()
                    is null
                        or ConfigurationSource.Convention
                )
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }

            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if ((Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) is { IsEnum: true } enumType)
                {
                    MapEnum(entityType, property, enumType);
                }
            }
        }
    }

    // camelCase text plus a CHECK constraint listing the allowed values (V-06).
    private static void MapEnum(IMutableEntityType entityType, IMutableProperty property, Type enumType)
    {
        Type converterType = typeof(CamelCaseEnumConverter<>).MakeGenericType(enumType);
        property.SetValueConverter((ValueConverter)Activator.CreateInstance(converterType)!);
        property.SetMaxLength(EnumMaxLength);

        var storedValues =
            (IEnumerable<string>)
                converterType.GetProperty(nameof(CamelCaseEnumConverter<>.StoredValues))!.GetValue(null)!;
        string column = property.GetColumnName();
        entityType.AddCheckConstraint(
            $"ck_{entityType.GetTableName()}_{column}_enum",
            $"{column} IN ({string.Join(", ", storedValues.Select(value => $"'{value}'"))})"
        );
    }

    private static bool IsAggregateRoot(Type type)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            {
                return true;
            }
        }

        return false;
    }
}
