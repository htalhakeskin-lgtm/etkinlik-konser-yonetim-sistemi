using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace FestOS.BuildingBlocks.UnitTests.Persistence;

public sealed class DatabaseConnectionsTests
{
    private const string BaseConnectionString =
        "Host=db;Port=5432;Database=festos;Username=postgres;Password=admin-secret";

    [Fact]
    public void ForRole_WithAPassword_ReplacesTheAdministratorCredentials()
    {
        IConfiguration configuration = Configuration(("Database:Passwords:festos_booking", "booking-secret"));

        var connection = new NpgsqlConnectionStringBuilder(
            DatabaseConnections.ForRole(configuration, "festos_booking")
        );

        connection.Host.ShouldBe("db");
        connection.Database.ShouldBe("festos");
        connection.Username.ShouldBe("festos_booking");
        connection.Password.ShouldBe("booking-secret");
    }

    [Fact]
    public void ForRole_WithoutAPassword_Throws()
    {
        IConfiguration configuration = Configuration();

        Should.Throw<InvalidOperationException>(() => DatabaseConnections.ForRole(configuration, "festos_booking"));
    }

    [Fact]
    public void ForAdministrator_WithoutAnAdminPassword_KeepsTheBaseCredentials()
    {
        IConfiguration configuration = Configuration();

        var connection = new NpgsqlConnectionStringBuilder(DatabaseConnections.ForAdministrator(configuration));

        connection.Username.ShouldBe("postgres");
        connection.Password.ShouldBe("admin-secret");
    }

    [Fact]
    public void ForAdministrator_WithAnAdminPassword_UsesIt()
    {
        IConfiguration configuration = Configuration(
            ("Database:AdminUsername", "festos_admin"),
            ("Database:AdminPassword", "other-secret")
        );

        var connection = new NpgsqlConnectionStringBuilder(DatabaseConnections.ForAdministrator(configuration));

        connection.Username.ShouldBe("festos_admin");
        connection.Password.ShouldBe("other-secret");
    }

    [Fact]
    public void Passwords_ForTheSection_ReturnsEveryRole()
    {
        IConfiguration configuration = Configuration(
            ("Database:Passwords:festos_migrator", "migrator-secret"),
            ("Database:Passwords:festos_booking", "booking-secret")
        );

        DatabaseConnections
            .Passwords(configuration)
            .Keys.Order(StringComparer.Ordinal)
            .ShouldBe(["festos_booking", "festos_migrator"]);
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection([
                new($"ConnectionStrings:{DatabaseConnections.ConnectionStringName}", BaseConnectionString),
                .. settings.Select(setting => new KeyValuePair<string, string?>(setting.Key, setting.Value)),
            ])
            .Build();
}
