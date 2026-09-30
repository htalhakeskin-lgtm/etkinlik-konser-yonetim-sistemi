using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace FestOS.DatabaseTests;

/// <summary>Who may do what with the change history (database §4, §14.2).</summary>
public sealed class AuditTableTests(PostgresDatabase database) : IAsyncLifetime
{
    private const string Insert = """
        INSERT INTO audit.audit_entries (id, occurred_at, actor_id, module, entity_type, entity_id, action, changes)
        VALUES (uuidv7(), now(), uuidv7(), 'sample', 'Probe', uuidv7(), 'created', '{}')
        """;

    private readonly Dictionary<string, string> _passwords = new(StringComparer.Ordinal)
    {
        [DatabaseRoles.Migrator] = Guid.CreateVersion7().ToString("N"),
        ["festos_audit"] = Guid.CreateVersion7().ToString("N"),
        ["festos_sample"] = Guid.CreateVersion7().ToString("N"),
    };

    public async ValueTask InitializeAsync()
    {
        await new DatabaseBootstrapper(NullLogger<DatabaseBootstrapper>.Instance).RunAsync(
            database.AdminConnectionString,
            new DatabaseBootstrapOptions(ModuleContexts.Schemas, _passwords),
            TestContext.Current.CancellationToken
        );

        foreach (ModuleDbContext context in ModuleContexts.Create(ConnectionString(DatabaseRoles.Migrator)))
        {
            await using (context)
            {
                await context.Database.MigrateAsync(TestContext.Current.CancellationToken);
            }
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [Trait("DatabaseRule", "DT-02")]
    [Trait("Rule", "BR-SYS-010")]
    [InlineData("festos_sample", Insert, true)]
    [InlineData("festos_sample", "SELECT count(*) FROM audit.audit_entries", false)]
    [InlineData("festos_sample", "UPDATE audit.audit_entries SET module = 'changed'", false)]
    [InlineData("festos_sample", "DELETE FROM audit.audit_entries", false)]
    [InlineData("festos_audit", "SELECT count(*) FROM audit.audit_entries", true)]
    [InlineData("festos_audit", "UPDATE audit.audit_entries SET module = 'changed'", false)]
    [InlineData("festos_audit", "DELETE FROM audit.audit_entries", false)]
    public async Task ModuleRoles_OnTheChangeHistory_OnlyAddEntriesAndOnlyAuditReads(
        string role,
        string sql,
        bool allowed
    )
    {
        await using var connection = new NpgsqlConnection(ConnectionString(role));
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        if (allowed)
        {
            await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }
        else
        {
            PostgresException exception = await Should.ThrowAsync<PostgresException>(() =>
                command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken)
            );
            exception.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
        }
    }

    private string ConnectionString(string role) =>
        new NpgsqlConnectionStringBuilder(database.AdminConnectionString)
        {
            Username = role,
            Password = _passwords[role],
            Pooling = false,
        }.ConnectionString;
}
