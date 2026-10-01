using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Identity.IntegrationTests;

/// <summary>A host with the Identity module on a migrated PostgreSQL database; each test resets it first.</summary>
public sealed class IdentityFixture : IAsyncLifetime
{
    private readonly PostgresDatabase _database = new();
    private IHost? _host;

    /// <summary>The user the host acts for.</summary>
    public FakeCurrentUser CurrentUser { get; } = new();

    /// <summary>The application services.</summary>
    public IServiceProvider Services => _host!.Services;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [$"ConnectionStrings:{DatabaseConnections.ConnectionStringName}"] = _database.AdminConnectionString,
                [$"Database:Passwords:{DatabaseRoles.Migrator}"] = Guid.CreateVersion7().ToString("N"),
                ["Database:Passwords:festos_audit"] = Guid.CreateVersion7().ToString("N"),
                ["Database:Passwords:festos_identity"] = Guid.CreateVersion7().ToString("N"),
            }
        );
        builder.Services.AddLogging();
        builder.Services.AddSingleton<ICurrentUser>(CurrentUser);
        builder.AddModules(new AuditModuleDefinition(), new IdentityModuleDefinition());
        _host = builder.Build();
        await _host.Services.GetRequiredService<DatabaseMigrator>().RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Empties the Identity tables and the change history.</summary>
    public Task ResetAsync() =>
        _database.ResetAsync(
            [AuditModuleDefinition.SchemaName, IdentityModuleDefinition.SchemaName],
            TestContext.Current.CancellationToken
        );

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _host?.Dispose();
        await _database.DisposeAsync();
    }
}
