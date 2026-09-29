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

    /// <summary>A superuser connection string for the <c>festos</c> database.</summary>
    public string AdminConnectionString => _container.GetConnectionString();

    /// <inheritdoc />
    public async ValueTask InitializeAsync() => await _container.StartAsync(TestContext.Current.CancellationToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
