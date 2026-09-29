using FestOS.Testing;
using Npgsql;

namespace FestOS.DatabaseTests;

/// <summary>Database-level settings (database §3, §17.1).</summary>
public sealed class DatabaseSetupTests(PostgresDatabase database)
{
    [Fact]
    [Trait("DatabaseRule", "DT-05")]
    public async Task Database_Always_UsesTheBuiltinCUtf8Collation()
    {
        await using NpgsqlConnection connection = await OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT datlocprovider, datlocale FROM pg_database WHERE datname = current_database()",
            connection
        );
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        (await reader.ReadAsync(TestContext.Current.CancellationToken)).ShouldBeTrue();
        reader.GetChar(0).ShouldBe('b', "b is the builtin locale provider");
        reader.GetString(1).ShouldBe("C.UTF-8");
    }

    [Fact]
    [Trait("DatabaseRule", "DT-05")]
    public async Task PublicSchema_Always_HoldsNoApplicationObjects()
    {
        // Relations and functions in public that do not belong to an extension.
        const string Sql = """
            SELECT c.relname
            FROM pg_class c
            WHERE c.relnamespace = 'public'::regnamespace
              AND NOT EXISTS (SELECT 1 FROM pg_depend d
                              WHERE d.classid = 'pg_class'::regclass AND d.objid = c.oid AND d.deptype = 'e')
            UNION ALL
            SELECT p.proname
            FROM pg_proc p
            WHERE p.pronamespace = 'public'::regnamespace
              AND NOT EXISTS (SELECT 1 FROM pg_depend d
                              WHERE d.classid = 'pg_proc'::regclass AND d.objid = p.oid AND d.deptype = 'e')
            """;

        await using NpgsqlConnection connection = await OpenAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        List<string> objects = [];
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            objects.Add(reader.GetString(0));
        }

        objects.ShouldBeEmpty();
    }

    private async Task<NpgsqlConnection> OpenAsync()
    {
        var connection = new NpgsqlConnection(database.AdminConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        return connection;
    }
}
