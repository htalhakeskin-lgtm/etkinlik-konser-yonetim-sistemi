using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Xunit;

namespace FestOS.Testing;

/// <summary>
/// One PostgreSQL 18 server per test project, shared through an xUnit assembly fixture and set up
/// like the development and demo databases: database <c>festos</c> with the builtin <c>C.UTF-8</c>
/// collation (database §3, testing §6).
/// </summary>
/// <example><c>[assembly: AssemblyFixture(typeof(PostgresDatabase))]</c></example>
public sealed class PostgresDatabase : IAsyncLifetime
{
    /// <summary>The image; the same major version as src/AppHost and the demo stack.</summary>
    public const string Image = "postgres:18";

    /// <summary>The database name used in every environment.</summary>
    public const string DatabaseName = "festos";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithDatabase(DatabaseName)
        .WithEnvironment("POSTGRES_INITDB_ARGS", "--locale-provider=builtin --builtin-locale=C.UTF-8")
        .Build();

    private Respawner? _respawner;

    /// <summary>A superuser connection string for the <c>festos</c> database.</summary>
    public string AdminConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Deletes every row in the given schemas except the migration history, so each test starts from
    /// the migrated, empty database (testing §6). Call it after the migrations have run.
    /// </summary>
    public async Task ResetAsync(IReadOnlyCollection<string> schemas, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(AdminConnectionString);
        await connection.OpenAsync(cancellationToken);
        _respawner ??= await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = [.. schemas],
                TablesToIgnore = [new Table("__ef_migrations_history")],
            }
        );
        await _respawner.ResetAsync(connection);
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync() => await _container.StartAsync(TestContext.Current.CancellationToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
