using Microsoft.Extensions.Configuration;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Builds the connection strings of the database roles from one base connection string and the role
/// passwords (building-blocks §5.2, security §6).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><c>ConnectionStrings:festos</c>: server, port and database. Credentials in it are the
/// administrator's and are used only by the bootstrap; development gets them from Aspire.</item>
/// <item><c>Database:AdminUsername</c> and <c>Database:AdminPassword</c>: the administrator when the
/// base connection string has no credentials, as in the demo environment.</item>
/// <item><c>Database:Passwords:{role}</c>: each role's password, from the secret store.</item>
/// </list>
/// </remarks>
public static class DatabaseConnections
{
    /// <summary>The name of the base connection string.</summary>
    public const string ConnectionStringName = "festos";

    private const string PasswordsSection = "Database:Passwords";

    /// <summary>The connection string of a role, with its own user name and password.</summary>
    public static string ForRole(IConfiguration configuration, string role)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string password =
            configuration[$"{PasswordsSection}:{role}"]
            ?? throw new InvalidOperationException($"No password is configured for the database role {role}.");

        return new NpgsqlConnectionStringBuilder(BaseConnectionString(configuration))
        {
            Username = role,
            Password = password,
        }.ConnectionString;
    }

    /// <summary>The administrator connection string for the bootstrap.</summary>
    public static string ForAdministrator(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var builder = new NpgsqlConnectionStringBuilder(BaseConnectionString(configuration));
        if (configuration["Database:AdminPassword"] is { } password)
        {
            builder.Username = configuration["Database:AdminUsername"] ?? "postgres";
            builder.Password = password;
        }

        return builder.ConnectionString;
    }

    /// <summary>Every configured role password, by role name.</summary>
    public static IReadOnlyDictionary<string, string> Passwords(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration
            .GetSection(PasswordsSection)
            .GetChildren()
            .Where(role => !string.IsNullOrEmpty(role.Value))
            .ToDictionary(role => role.Key, role => role.Value!, StringComparer.Ordinal);
    }

    private static string BaseConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString(ConnectionStringName)
        ?? throw new InvalidOperationException($"The connection string '{ConnectionStringName}' is not configured.");
}
