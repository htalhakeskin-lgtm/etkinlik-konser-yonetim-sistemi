using Microsoft.Extensions.Logging;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Prepares the database before the module migrations run: roles and their passwords, memberships,
/// role-level settings, database privileges and extensions (building-blocks §5.1, database §3, §4,
/// §17). Runs with an administrator connection as the first step of <c>migrate</c>; safe to run again,
/// which is also how passwords are rotated.
/// </summary>
public sealed partial class DatabaseBootstrapper(ILogger<DatabaseBootstrapper> logger)
{
    private const string LoginRoleAttributes = "NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS INHERIT";

    private static readonly string[] Extensions = ["btree_gist", "pg_trgm"];

    // Every login role: a transaction left open does not hold locks for long (database §17).
    private static readonly (string Name, string Value)[] CommonSettings =
    [
        ("idle_in_transaction_session_timeout", "60s"),
    ];

    private static readonly (string Name, string Value)[] ModuleSettings =
    [
        ("statement_timeout", "30s"),
        ("lock_timeout", "10s"),
    ];

    // Objects the migrator creates belong to the owner; schema changes do not stall live traffic.
    private static readonly (string Name, string Value)[] MigratorSettings =
    [
        ("role", DatabaseRoles.Owner),
        ("statement_timeout", "0"),
        ("lock_timeout", "5s"),
    ];

    private static readonly (string Name, string Value)[] ReadOnlySettings =
    [
        ("statement_timeout", "30s"),
        ("default_transaction_read_only", "on"),
    ];

    private static readonly (string Name, string Value)[] MonitorSettings = [("statement_timeout", "30s")];

    /// <summary>Runs the bootstrap in one transaction.</summary>
    public async Task RunAsync(
        string adminConnectionString,
        DatabaseBootstrapOptions options,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sql = new BootstrapSql(connection, cancellationToken);

        // Two migrate runs at the same time wait for each other.
        await sql.ExecuteAsync("SELECT pg_advisory_xact_lock(hashtext('festos.bootstrap'))");

        await EnsureRoleAsync(sql, DatabaseRoles.Owner, password: null);
        await EnsureRoleAsync(sql, DatabaseRoles.AuditWriter, password: null);
        await EnsureLoginRoleAsync(sql, DatabaseRoles.Migrator, RequiredPassword(options, DatabaseRoles.Migrator));
        await EnsureLoginRoleAsync(
            sql,
            DatabaseRoles.ReadOnly,
            options.Passwords.GetValueOrDefault(DatabaseRoles.ReadOnly)
        );
        await EnsureLoginRoleAsync(
            sql,
            DatabaseRoles.Monitor,
            options.Passwords.GetValueOrDefault(DatabaseRoles.Monitor)
        );

        await sql.ExecuteFormattedAsync("GRANT %I TO %I", DatabaseRoles.Owner, DatabaseRoles.Migrator);
        await sql.ExecuteFormattedAsync("GRANT pg_read_all_data TO %I", DatabaseRoles.ReadOnly);
        await sql.ExecuteFormattedAsync("GRANT pg_monitor TO %I", DatabaseRoles.Monitor);

        await ApplySettingsAsync(sql, DatabaseRoles.Migrator, MigratorSettings);
        await ApplySettingsAsync(sql, DatabaseRoles.ReadOnly, ReadOnlySettings);
        await ApplySettingsAsync(sql, DatabaseRoles.Monitor, MonitorSettings);

        // Only the listed roles may connect; the owner creates the module schemas.
        await sql.ExecuteFormattedAsync("REVOKE ALL ON DATABASE %I FROM PUBLIC", await sql.DatabaseNameAsync());
        await sql.ExecuteFormattedAsync(
            "GRANT CREATE ON DATABASE %I TO %I",
            await sql.DatabaseNameAsync(),
            DatabaseRoles.Owner
        );
        foreach (string role in new[] { DatabaseRoles.Migrator, DatabaseRoles.ReadOnly, DatabaseRoles.Monitor })
        {
            await sql.ExecuteFormattedAsync("GRANT CONNECT ON DATABASE %I TO %I", await sql.DatabaseNameAsync(), role);
        }

        foreach (string schema in options.ModuleSchemas)
        {
            string role = DatabaseRoles.ForModule(schema);
            await EnsureLoginRoleAsync(sql, role, RequiredPassword(options, role));
            await ApplySettingsAsync(sql, role, ModuleSettings);
            await sql.ExecuteFormattedAsync("ALTER ROLE %I SET search_path TO %I, public", role, schema);
            await sql.ExecuteFormattedAsync("GRANT %I TO %I", DatabaseRoles.AuditWriter, role);
            await sql.ExecuteFormattedAsync("GRANT CONNECT ON DATABASE %I TO %I", await sql.DatabaseNameAsync(), role);
        }

        foreach (string extension in Extensions)
        {
            await sql.ExecuteFormattedAsync("CREATE EXTENSION IF NOT EXISTS %I WITH SCHEMA public", extension);
        }

        await transaction.CommitAsync(cancellationToken);
        LogFinished(logger, options.ModuleSchemas.Count);
    }

    private static string RequiredPassword(DatabaseBootstrapOptions options, string role) =>
        options.Passwords.TryGetValue(role, out string? password) && !string.IsNullOrEmpty(password)
            ? password
            : throw new InvalidOperationException($"No password is configured for the database role {role}.");

    private async Task EnsureLoginRoleAsync(BootstrapSql sql, string role, string? password)
    {
        await EnsureRoleAsync(sql, role, password);
        await ApplySettingsAsync(sql, role, CommonSettings);
    }

    // A role without a password cannot log in; with one, the password is set again (rotation).
    private async Task EnsureRoleAsync(BootstrapSql sql, string role, string? password)
    {
        if (!await sql.RoleExistsAsync(role))
        {
            await sql.ExecuteFormattedAsync("CREATE ROLE %I", role);
            LogRoleCreated(logger, role);
        }

        if (string.IsNullOrEmpty(password))
        {
            await sql.ExecuteFormattedAsync($"ALTER ROLE %I WITH NOLOGIN {LoginRoleAttributes}", role);
        }
        else
        {
            await sql.ExecuteFormattedAsync(
                $"ALTER ROLE %I WITH LOGIN {LoginRoleAttributes} PASSWORD %L",
                role,
                password
            );
        }
    }

    private static async Task ApplySettingsAsync(BootstrapSql sql, string role, (string Name, string Value)[] settings)
    {
        foreach ((string name, string value) in settings)
        {
            await sql.ExecuteFormattedAsync("ALTER ROLE %I SET %s = %L", role, name, value);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Created database role {RoleName}")]
    private static partial void LogRoleCreated(ILogger logger, string roleName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database bootstrap finished for {ModuleCount} modules")]
    private static partial void LogFinished(ILogger logger, int moduleCount);
}
