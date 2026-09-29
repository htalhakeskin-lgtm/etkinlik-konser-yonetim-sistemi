using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace FestOS.DatabaseTests;

/// <summary>Roles, settings and privileges written by the database bootstrap (database §4, §17).</summary>
public sealed class DatabaseBootstrapTests(PostgresDatabase database)
{
    private static readonly string[] ModuleSchemas = ["booking", "planning"];

    // Fresh passwords for every test; the monitor role gets none, so it must stay unable to log in.
    private readonly Dictionary<string, string> _passwords = new(StringComparer.Ordinal)
    {
        [DatabaseRoles.Migrator] = NewPassword(),
        [DatabaseRoles.ReadOnly] = NewPassword(),
        ["festos_booking"] = NewPassword(),
        ["festos_planning"] = NewPassword(),
    };

    [Fact]
    [Trait("DatabaseRule", "DT-02")]
    public async Task Bootstrap_ForEveryRole_SetsWhetherItCanLogIn()
    {
        await BootstrapAsync();

        List<(string Role, bool CanLogIn)> roles = await QueryAsync(
            "SELECT rolname, rolcanlogin FROM pg_roles WHERE rolname LIKE 'festos%'",
            reader => (reader.GetString(0), reader.GetBoolean(1))
        );
        var canLogIn = roles.ToDictionary(role => role.Role, role => role.CanLogIn, StringComparer.Ordinal);

        canLogIn[DatabaseRoles.Owner].ShouldBeFalse();
        canLogIn[DatabaseRoles.Migrator].ShouldBeTrue();
        canLogIn[DatabaseRoles.ReadOnly].ShouldBeTrue();
        canLogIn[DatabaseRoles.Monitor].ShouldBeFalse();
        canLogIn["festos_booking"].ShouldBeTrue();
        canLogIn["festos_planning"].ShouldBeTrue();
    }

    [Theory]
    [Trait("DatabaseRule", "DT-02")]
    [InlineData(DatabaseRoles.Migrator, DatabaseRoles.Owner, true)]
    [InlineData(DatabaseRoles.ReadOnly, "pg_read_all_data", true)]
    [InlineData(DatabaseRoles.Monitor, "pg_monitor", true)]
    [InlineData("festos_booking", DatabaseRoles.Owner, false)]
    [InlineData("festos_booking", "pg_read_all_data", false)]
    public async Task Bootstrap_ForEveryRole_GrantsOnlyItsMemberships(string role, string group, bool isMember)
    {
        await BootstrapAsync();

        List<bool> result = await QueryAsync(
            "SELECT pg_has_role($1, $2, 'MEMBER')",
            reader => reader.GetBoolean(0),
            role,
            group
        );

        result.ShouldHaveSingleItem().ShouldBe(isMember);
    }

    [Theory]
    [InlineData("festos_booking", "search_path=booking, public")]
    [InlineData("festos_booking", "statement_timeout=30s")]
    [InlineData("festos_booking", "lock_timeout=10s")]
    [InlineData("festos_booking", "idle_in_transaction_session_timeout=60s")]
    [InlineData(DatabaseRoles.Migrator, "role=festos_owner")]
    [InlineData(DatabaseRoles.Migrator, "statement_timeout=0")]
    [InlineData(DatabaseRoles.Migrator, "lock_timeout=5s")]
    [InlineData(DatabaseRoles.ReadOnly, "default_transaction_read_only=on")]
    public async Task Bootstrap_ForEveryRole_WritesItsSettings(string role, string setting)
    {
        await BootstrapAsync();

        List<string[]> settings = await QueryAsync(
            """
            SELECT s.setconfig FROM pg_db_role_setting s
            JOIN pg_roles r ON r.oid = s.setrole
            WHERE r.rolname = $1 AND s.setdatabase = 0
            """,
            reader => reader.GetFieldValue<string[]>(0),
            role
        );

        settings.ShouldHaveSingleItem().ShouldContain(value => string.Equals(value, setting, StringComparison.Ordinal));
    }

    [Fact]
    [Trait("DatabaseRule", "DT-02")]
    public async Task ModuleRole_WhenConnected_UsesItsSchemaAndCannotCreateInPublic()
    {
        await BootstrapAsync();
        await using NpgsqlConnection connection = await OpenAsync("festos_booking");

        await using var searchPath = new NpgsqlCommand("SHOW search_path", connection);
        (await searchPath.ExecuteScalarAsync(TestContext.Current.CancellationToken)).ShouldBe("booking, public");

        await using var create = new NpgsqlCommand("CREATE TABLE public.probe (probe_id integer)", connection);
        PostgresException exception = await Should.ThrowAsync<PostgresException>(() =>
            create.ExecuteNonQueryAsync(TestContext.Current.CancellationToken)
        );
        exception.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task Migrator_WhenConnected_CreatesObjectsOwnedByTheOwner()
    {
        await BootstrapAsync();
        await using NpgsqlConnection connection = await OpenAsync(DatabaseRoles.Migrator);

        await using var create = new NpgsqlCommand(
            """
            CREATE SCHEMA probe;
            SELECT nspowner::regrole::text FROM pg_namespace WHERE nspname = 'probe';
            """,
            connection
        );
        object? owner = await create.ExecuteScalarAsync(TestContext.Current.CancellationToken);
        await using var drop = new NpgsqlCommand("DROP SCHEMA probe", connection);
        await drop.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);

        owner.ShouldBe(DatabaseRoles.Owner);
    }

    [Fact]
    public async Task Bootstrap_RunAgainWithANewPassword_ReplacesTheOldOne()
    {
        await BootstrapAsync();
        string oldPassword = _passwords["festos_booking"];
        _passwords["festos_booking"] = NewPassword();

        await BootstrapAsync();

        await using (await OpenAsync("festos_booking")) { }
        PostgresException exception = await Should.ThrowAsync<PostgresException>(() =>
            OpenAsync("festos_booking", oldPassword)
        );
        exception.SqlState.ShouldBe(PostgresErrorCodes.InvalidPassword);
    }

    [Fact]
    public async Task Bootstrap_Always_InstallsTheExtensions()
    {
        await BootstrapAsync();

        List<string> extensions = await QueryAsync(
            "SELECT extname FROM pg_extension WHERE extnamespace = 'public'::regnamespace",
            reader => reader.GetString(0)
        );

        extensions.Order(StringComparer.Ordinal).ShouldBe(["btree_gist", "pg_trgm"]);
    }

    [Fact]
    public async Task Bootstrap_WithoutAModulePassword_Throws()
    {
        _passwords.Remove("festos_planning");

        await Should.ThrowAsync<InvalidOperationException>(BootstrapAsync);
    }

    private static string NewPassword() => Guid.CreateVersion7().ToString("N");

    private Task BootstrapAsync() =>
        new DatabaseBootstrapper(NullLogger<DatabaseBootstrapper>.Instance).RunAsync(
            database.AdminConnectionString,
            new DatabaseBootstrapOptions(ModuleSchemas, _passwords),
            TestContext.Current.CancellationToken
        );

    private async Task<NpgsqlConnection> OpenAsync(string role, string? password = null)
    {
        var builder = new NpgsqlConnectionStringBuilder(database.AdminConnectionString)
        {
            Username = role,
            Password = password ?? _passwords[role],
            Pooling = false,
        };
        var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        return connection;
    }

    private async Task<List<T>> QueryAsync<T>(string sql, Func<NpgsqlDataReader, T> read, params string[] arguments)
    {
        await using var connection = new NpgsqlConnection(database.AdminConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (string argument in arguments)
        {
            command.Parameters.Add(new NpgsqlParameter<string> { TypedValue = argument });
        }

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        List<T> rows = [];
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            rows.Add(read(reader));
        }

        return rows;
    }
}
