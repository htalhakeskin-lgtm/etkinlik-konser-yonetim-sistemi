using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Inventory.Infrastructure;
using FestOS.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace FestOS.Modules.Inventory.IntegrationTests;

/// <summary>The Inventory module on a migrated PostgreSQL database. Each test resets the database first.</summary>
public sealed class InventoryFixture : IAsyncLifetime
{
    private readonly PostgresDatabase _database = new();
    private readonly Dictionary<string, string?> _settings = new(StringComparer.Ordinal);
    private IHost? _host;

    /// <summary>The clock of the host; starts on a microsecond-aligned Monday morning in Istanbul.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero));

    /// <summary>The user the host acts for.</summary>
    public FakeCurrentUser CurrentUser { get; } = new();

    /// <summary>The host's services.</summary>
    public IServiceProvider Services => _host!.Services;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        _settings[$"ConnectionStrings:{DatabaseConnections.ConnectionStringName}"] = _database.AdminConnectionString;
        _settings[$"Database:Passwords:{DatabaseRoles.Migrator}"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_audit"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_inventory"] = Guid.CreateVersion7().ToString("N");

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(_settings);
        builder.Services.AddLogging();
        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.Services.AddSingleton<ICurrentUser>(CurrentUser);
        builder.AddModules(new AuditModuleDefinition(), new InventoryModuleDefinition());
        _host = builder.Build();
        await _host.Services.GetRequiredService<DatabaseMigrator>().RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Empties the Inventory tables and the change history.</summary>
    public Task ResetAsync() =>
        _database.ResetAsync(
            [AuditModuleDefinition.SchemaName, InventoryModuleDefinition.SchemaName],
            TestContext.Current.CancellationToken
        );

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _host?.Dispose();
        await _database.DisposeAsync();
    }
}
