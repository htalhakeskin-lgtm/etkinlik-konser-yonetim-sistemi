using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Registers a module's database context and its connection settings (building-blocks §5.2).</summary>
public static class ModuleDbContextExtensions
{
    /// <summary>The name of each schema's migration history table (naming §5.1).</summary>
    public const string MigrationsHistoryTable = "__ef_migrations_history";

    /// <summary>
    /// Registers the context with the module role's connection (<see cref="DatabaseConnections.ForRole"/>)
    /// and the pool size <c>Modules:{moduleName}:Database:MaxPoolSize</c>, and lets the migrate command
    /// apply the module's migrations.
    /// </summary>
    public static IHostApplicationBuilder AddModuleDbContext<TContext>(
        this IHostApplicationBuilder builder,
        string moduleName,
        string schema
    )
        where TContext : ModuleDbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDbContext<TContext>(
            (services, options) =>
            {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();
                options.UseModuleDatabase(
                    DatabaseConnections.ForRole(configuration, DatabaseRoles.ForModule(schema)),
                    schema,
                    configuration.GetValue<int?>($"Modules:{moduleName}:Database:MaxPoolSize")
                );
            }
        );
        builder.Services.AddSingleton(
            new ModuleDatabase(
                schema,
                (connectionString, cancellationToken) =>
                    MigrateAsync<TContext>(connectionString, schema, cancellationToken)
            )
        );

        return builder;
    }

    /// <summary>
    /// The shared connection settings: application name <c>festos-{schema}</c>, a 30 second command
    /// timeout, the retry strategy, the history table in the module's schema and snake_case names.
    /// Also used by the migrate command and the design-time factories.
    /// </summary>
    public static DbContextOptionsBuilder UseModuleDatabase(
        this DbContextOptionsBuilder options,
        string connectionString,
        string schema,
        int? maxPoolSize = null
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        var connection = new NpgsqlConnectionStringBuilder(connectionString)
        {
            ApplicationName = "festos-" + schema,
            CommandTimeout = 30,
        };
        if (maxPoolSize is { } size)
        {
            connection.MaxPoolSize = size;
        }

        return options
            .UseNpgsql(
                connection.ConnectionString,
                npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, schema).EnableRetryOnFailure()
            )
            .UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
    }

    private static async Task MigrateAsync<TContext>(
        string connectionString,
        string schema,
        CancellationToken cancellationToken
    )
        where TContext : ModuleDbContext
    {
        var options = new DbContextOptionsBuilder<TContext>();
        options.UseModuleDatabase(connectionString, schema);

        // Module contexts take their options as the only constructor parameter.
        await using var context = (TContext)Activator.CreateInstance(typeof(TContext), options.Options)!;
        await context.Database.MigrateAsync(cancellationToken);
    }
}
