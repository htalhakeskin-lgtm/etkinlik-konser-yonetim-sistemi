using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>
/// A host with the sample module (building-blocks BB-08) on a migrated PostgreSQL database, shared by
/// the platform integration tests. Each test calls <see cref="ResetAsync"/> first.
/// </summary>
public sealed class SampleModuleFixture : IAsyncLifetime
{
    private readonly PostgresDatabase _database = new();
    private IHost? _host;

    /// <summary>The clock; starts on a microsecond-aligned Monday morning in Istanbul (testing §11).</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero));

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
                ["Database:Passwords:festos_sample"] = Guid.CreateVersion7().ToString("N"),
            }
        );
        builder.Services.AddLogging();
        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.Services.AddSingleton<ICurrentUser>(CurrentUser);
        builder.AddModules(new AuditModuleDefinition(), new SampleModuleDefinition());
        _host = builder.Build();

        await _host.Services.GetRequiredService<DatabaseMigrator>().RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Empties the sample module's tables and the change history, and restores the default user.</summary>
    public async Task ResetAsync()
    {
        CurrentUser.UserId = FakeCurrentUser.DefaultUserId;
        await _database.ResetAsync(
            [AuditModuleDefinition.SchemaName, SampleModuleDefinition.SchemaName],
            TestContext.Current.CancellationToken
        );
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _host?.Dispose();
        await _database.DisposeAsync();
    }
}
