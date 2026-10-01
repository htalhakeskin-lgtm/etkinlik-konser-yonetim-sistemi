using System.Reflection;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace FestOS.DatabaseTests;

/// <summary>Catalog checks on every module's migrated schema (database §12.1, §17.1; DT-03, DT-04).</summary>
public sealed class SchemaRuleTests(PostgresDatabase database) : IAsyncLifetime
{
    // Reference columns without a foreign key that need no index, with the reason (DT-03).
    private static readonly Dictionary<string, string> UnindexedReferences = new(StringComparer.Ordinal)
    {
        ["audit.audit_entries.entity_id"] =
            "A record's history is read by entity type and id together; the (entity_type, entity_id, occurred_at) index serves it.",
        ["audit.audit_entries.trace_id"] = "An OpenTelemetry trace id, not a record; it links a change to its log.",
        ["audit.audit_entries.actor_id"] =
            "Nothing reads history by user yet; the reading screens of 1.2 add the index if they filter by user.",
        ["sample.sample_usage_records.sample_item_id"] = "The test-only sample module never queries by it.",
    };

    // Constraints that guard a technical value, not a business rule, with the reason (DT-04).
    private static readonly Dictionary<string, string> ConstraintsWithoutRule = new(StringComparer.Ordinal)
    {
        ["ux_sessions_key_hash"] = "A random key's hash; a clash is impossible in practice and is no user's mistake.",
    };

    private readonly Dictionary<string, string> _passwords = ModuleContexts.NewPasswords();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await new DatabaseBootstrapper(NullLogger<DatabaseBootstrapper>.Instance).RunAsync(
            database.AdminConnectionString,
            new DatabaseBootstrapOptions(ModuleContexts.Schemas, _passwords),
            Cancellation
        );
        foreach (ModuleDbContext context in ModuleContexts.Create(MigratorConnectionString()))
        {
            await using (context)
            {
                await context.Database.MigrateAsync(Cancellation);
            }
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    [Trait("DatabaseRule", "DT-03")]
    public async Task ReferenceColumnsWithoutForeignKey_HaveAnIndex()
    {
        List<string> unindexed = await QueryAsync(
            """
            SELECT n.nspname || '.' || t.relname || '.' || a.attname
            FROM pg_attribute a
            JOIN pg_class t ON t.oid = a.attrelid AND t.relkind = 'r'
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = ANY(@schemas)
              AND a.attnum > 0 AND NOT a.attisdropped
              AND a.attname LIKE '%\_id' ESCAPE '\'
              AND NOT EXISTS (SELECT 1 FROM pg_constraint f
                              WHERE f.conrelid = t.oid AND f.contype = 'f' AND a.attnum = ANY (f.conkey))
              AND NOT EXISTS (SELECT 1 FROM pg_index i WHERE i.indrelid = t.oid AND i.indkey[0] = a.attnum)
            """,
            ("schemas", ModuleContexts.Schemas.ToArray())
        );

        unindexed.Where(column => !UnindexedReferences.ContainsKey(column)).ShouldBeEmpty();
    }

    [Fact]
    [Trait("DatabaseRule", "DT-04")]
    public async Task CheckUniqueAndExclusionConstraints_MapToARuleCode()
    {
        List<string> unmapped = [];
        foreach (ModuleDbContext context in ModuleContexts.Create(ModuleContexts.ModelOnlyConnectionString))
        {
            await using (context)
            {
                IReadOnlyDictionary<string, string> rules = ConstraintRulesOf(context);

                // Unique indexes count as unique constraints; enum checks guard the stored names, not a rule
                // (database §6.2).
                List<string> constraints = await QueryAsync(
                    """
                    SELECT con.conname
                    FROM pg_constraint con JOIN pg_namespace n ON n.oid = con.connamespace
                    WHERE n.nspname = @schema AND con.contype IN ('c', 'u', 'x')
                    UNION
                    SELECT ic.relname
                    FROM pg_index i
                    JOIN pg_class ic ON ic.oid = i.indexrelid
                    JOIN pg_namespace n ON n.oid = ic.relnamespace
                    WHERE n.nspname = @schema AND i.indisunique AND NOT i.indisprimary
                      AND NOT EXISTS (SELECT 1 FROM pg_constraint c WHERE c.conindid = i.indexrelid)
                    """,
                    ("schema", context.Schema)
                );

                unmapped.AddRange(
                    constraints
                        .Where(name => !name.EndsWith("_enum", StringComparison.Ordinal))
                        .Where(name => !rules.ContainsKey(name) && !ConstraintsWithoutRule.ContainsKey(name))
                        .Select(name => $"{context.Schema}.{name}")
                );
            }
        }

        unmapped.ShouldBeEmpty();
    }

    [Fact]
    public async Task IdentityMigrations_SeedTheSystemUserWhoCannotSignIn()
    {
        List<string> seeded = await QueryAsync(
            "SELECT full_name || '|' || password_hash FROM identity.users WHERE id = @id",
            ("id", SystemUser.Id)
        );

        seeded.ShouldBe(["Sistem|"]);
    }

    // The mapping is the context's own (protected) table of constraint names and rule codes.
    private static IReadOnlyDictionary<string, string> ConstraintRulesOf(ModuleDbContext context) =>
        (IReadOnlyDictionary<string, string>)
            typeof(ModuleDbContext)
                .GetProperty("ConstraintRules", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(context)!;

    private async Task<List<string>> QueryAsync(string sql, (string Name, object Value) parameter)
    {
        await using var connection = new NpgsqlConnection(database.AdminConnectionString);
        await connection.OpenAsync(Cancellation);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Cancellation);
        while (await reader.ReadAsync(Cancellation))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }

    private string MigratorConnectionString() =>
        new NpgsqlConnectionStringBuilder(database.AdminConnectionString)
        {
            Username = DatabaseRoles.Migrator,
            Password = _passwords[DatabaseRoles.Migrator],
            Pooling = false,
        }.ConnectionString;
}
