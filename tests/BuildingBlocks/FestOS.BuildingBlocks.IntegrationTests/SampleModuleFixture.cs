using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>
/// A host with the sample module (building-blocks BB-08) on a migrated PostgreSQL database, shared by
/// the platform integration tests. Each test calls <see cref="ResetAsync"/> first. The host is built but
/// not started, so no dispatcher runs unless a test starts a host of its own.
/// </summary>
public sealed class SampleModuleFixture : IAsyncLifetime
{
    private readonly PostgresDatabase _database = new();
    private readonly Dictionary<string, string?> _settings = new(StringComparer.Ordinal);
    private IHost? _host;

    /// <summary>The clock; starts on a microsecond-aligned Monday morning in Istanbul (testing §11).</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero));

    /// <summary>The user the host acts for.</summary>
    public FakeCurrentUser CurrentUser { get; } = new();

    /// <summary>The base connection string, with the administrator's credentials.</summary>
    public string ConnectionString => _database.AdminConnectionString;

    /// <summary>The application services.</summary>
    public IServiceProvider Services => _host!.Services;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        _settings[$"ConnectionStrings:{DatabaseConnections.ConnectionStringName}"] = _database.AdminConnectionString;
        _settings[$"Database:Passwords:{DatabaseRoles.Migrator}"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_audit"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_sample"] = Guid.CreateVersion7().ToString("N");

        _host = CreateHost(Time);
        await _host.Services.GetRequiredService<DatabaseMigrator>().RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Builds another host on the same database with its own clock, e.g. to start it and watch the
    /// dispatcher or a job work; the caller starts, stops and disposes it. <paramref name="settings"/>
    /// override the shared configuration.
    /// </summary>
    public IHost CreateHost(
        TimeProvider time,
        Action<IServiceCollection>? configure = null,
        IReadOnlyDictionary<string, string?>? settings = null
    )
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(_settings);
        builder.Configuration.AddInMemoryCollection(
            settings ?? new Dictionary<string, string?>(StringComparer.Ordinal)
        );
        builder.Services.AddLogging();
        builder.Services.AddSingleton(time);
        builder.Services.AddSingleton<ICurrentUser>(CurrentUser);
        builder.AddModules(new AuditModuleDefinition(), new SampleModuleDefinition());
        configure?.Invoke(builder.Services);
        return builder.Build();
    }

    /// <summary>
    /// Starts a web application wired like the Host, with the modules' endpoints on the same database,
    /// served in memory, e.g. to test request headers end to end; the caller disposes it.
    /// </summary>
    public async Task<WebApplication> StartWebApplicationAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(_settings);
        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.Services.AddSingleton<ICurrentUser>(CurrentUser);
        builder.AddHttpPlatform();
        builder.AddRealtime();
        builder.AddModules(new AuditModuleDefinition(), new SampleModuleDefinition());
        SignedInTestUser.Register(builder.Services);

        WebApplication app = builder.Build();
        app.UseHttpPlatform();
        app.UseAuthentication();
        app.UseCsrfProtection();
        app.UseAuthorization();
        app.MapAntiforgeryToken();
        app.MapRealtime();
        app.MapModules();
        await app.StartAsync(TestContext.Current.CancellationToken);
        return app;
    }

    /// <summary>
    /// A client for the web application that sends requests like the front end: over HTTPS, with the
    /// cookies, the antiforgery token and an idempotency key (testing §6).
    /// </summary>
    public static HttpClient CreateClient(WebApplication app) =>
        new(new BrowserLikeHandler { InnerHandler = app.GetTestServer().CreateHandler() })
        {
            BaseAddress = new Uri("https://localhost/"),
        };

    /// <summary>Empties the sample module's tables and the change history, and restores the default user and probe.</summary>
    public async Task ResetAsync()
    {
        CurrentUser.UserId = FakeCurrentUser.DefaultUserId;
        Services.GetRequiredService<SampleListenerProbe>().Reset();
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
