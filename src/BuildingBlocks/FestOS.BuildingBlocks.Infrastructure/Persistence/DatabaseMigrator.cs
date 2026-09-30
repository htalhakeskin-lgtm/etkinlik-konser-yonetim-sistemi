using FestOS.BuildingBlocks.Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Prepares the database and applies every module's migrations (building-blocks §5.4): the
/// <c>migrate</c> command in the demo and production environments, the Host's startup in development.
/// </summary>
public sealed partial class DatabaseMigrator(
    IConfiguration configuration,
    ModuleCatalog modules,
    IEnumerable<ModuleDatabase> databases,
    DatabaseBootstrapper bootstrapper,
    ILogger<DatabaseMigrator> logger
)
{
    /// <summary>Runs the bootstrap with the administrator, then the migrations with the migrator role.</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await bootstrapper.RunAsync(
            DatabaseConnections.ForAdministrator(configuration),
            new DatabaseBootstrapOptions(
                [.. modules.Modules.Select(module => module.Schema)],
                DatabaseConnections.Passwords(configuration)
            ),
            cancellationToken
        );

        string migratorConnectionString = DatabaseConnections.ForRole(configuration, DatabaseRoles.Migrator);
        foreach (ModuleDatabase database in databases)
        {
            await database.Migrate(migratorConnectionString, cancellationToken);
            LogMigrated(logger, database.Schema);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applied the migrations of schema {SchemaName}")]
    private static partial void LogMigrated(ILogger logger, string schemaName);
}
