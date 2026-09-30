using System.Text;
using System.Text.RegularExpressions;
using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FestOS.DatabaseTests;

/// <summary>
/// The model every module produces with the shared rules (building-blocks §5.3). These read the
/// model only; no database is needed.
/// </summary>
public sealed partial class ModuleModelTests
{
    [Fact]
    [Trait("ArchitectureRule", "AT-13")]
    public void Names_InEveryModule_AreSnakeCaseFitPostgresAndUseTheirPrefix()
    {
        List<string> violations = [];

        foreach (ITable table in RelationalModels().SelectMany(model => model.Tables))
        {
            Check(table.Name, prefix: null);
            violations.AddRange(table.Columns.Select(column => column.Name).Where(name => !IsValidName(name)));
            Check(table.PrimaryKey?.Name, "pk_");
            table.ForeignKeyConstraints.ToList().ForEach(foreignKey => Check(foreignKey.Name, "fk_"));
            table.Indexes.ToList().ForEach(index => Check(index.Name, index.IsUnique ? "ux_" : "ix_"));
            table.CheckConstraints.ToList().ForEach(constraint => Check(constraint.Name, "ck_"));
        }

        violations.ShouldBeEmpty();

        void Check(string? name, string? prefix)
        {
            if (
                name is not null
                && (!IsValidName(name) || (prefix is not null && !name.StartsWith(prefix, StringComparison.Ordinal)))
            )
            {
                violations.Add(name);
            }
        }
    }

    [Fact]
    [Trait("ArchitectureRule", "AT-13")]
    public void TextColumns_InEveryModule_HaveALengthOrAreText()
    {
        List<string> unbounded =
        [
            .. Models()
                .SelectMany(model => model.GetEntityTypes())
                .SelectMany(entityType => entityType.GetProperties())
                .Where(property => property.ClrType == typeof(string))
                .Where(property =>
                    property.GetMaxLength() is null && property.GetColumnType() is not ("text" or "jsonb")
                )
                .Select(property => $"{property.DeclaringType.DisplayName()}.{property.Name}"),
        ];

        unbounded.ShouldBeEmpty();
    }

    [Fact]
    public void Conventions_ForTheSampleModule_ProduceTheSharedRules()
    {
        using SampleDbContext context = ModuleContexts.Create<SampleDbContext>(
            ModuleContexts.ModelOnlyConnectionString,
            SampleModuleDefinition.SchemaName,
            options => new(options)
        );
        IModel model = context.GetService<IDesignTimeModel>().Model;
        IEntityType item = model.FindEntityType(typeof(SampleItem)).ShouldNotBeNull();
        IEntityType part = model.FindEntityType(typeof(SampleItemPart)).ShouldNotBeNull();

        item.GetSchema().ShouldBe("sample");
        item.FindPrimaryKey()!.Properties.ShouldHaveSingleItem().ValueGenerated.ShouldBe(ValueGenerated.Never);
        item.FindProperty(nameof(SampleItem.Version))!.IsConcurrencyToken.ShouldBeTrue();
        item.FindProperty(nameof(SampleItem.UnitPrice))!.GetColumnType().ShouldBe("numeric(19,4)");
        item.GetCheckConstraints().ShouldHaveSingleItem().Sql.ShouldBe("status IN ('draft', 'inUse')");
        part.GetForeignKeys()
            .ToDictionary(
                foreignKey => foreignKey.PrincipalEntityType.ClrType.Name,
                foreignKey => foreignKey.DeleteBehavior,
                StringComparer.Ordinal
            )
            .ShouldBe(
                new Dictionary<string, DeleteBehavior>(StringComparer.Ordinal)
                {
                    [nameof(SampleItem)] = DeleteBehavior.Cascade,
                    [nameof(SampleItemPart)] = DeleteBehavior.Restrict,
                },
                ignoreOrder: true
            );
    }

    [Fact]
    public void AuditEntries_InEveryModule_AreMappedButOnlyTheAuditModuleCreatesTheTable()
    {
        foreach (IModel model in Models())
        {
            IEntityType auditEntry = model.FindEntityType(typeof(AuditEntry)).ShouldNotBeNull();
            bool ownsTable = string.Equals(model.GetDefaultSchema(), AuditEntry.SchemaName, StringComparison.Ordinal);

            auditEntry.GetSchema().ShouldBe(AuditEntry.SchemaName);
            auditEntry.IsTableExcludedFromMigrations().ShouldBe(!ownsTable);
        }
    }

    private static bool IsValidName(string name) => SnakeCase().IsMatch(name) && Encoding.UTF8.GetByteCount(name) <= 63;

    private static IEnumerable<IModel> Models() =>
        ModuleContexts
            .Create(ModuleContexts.ModelOnlyConnectionString)
            .Select(context =>
            {
                using (context)
                {
                    return context.GetService<IDesignTimeModel>().Model;
                }
            });

    private static IEnumerable<IRelationalModel> RelationalModels() =>
        Models().Select(model => model.GetRelationalModel());

    [GeneratedRegex("^[a-z][a-z0-9_]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex SnakeCase();
}
