using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Catalog.Infrastructure;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Inventory.Infrastructure;
using FestOS.Modules.Parties.Infrastructure;
using FestOS.Modules.Riders.Infrastructure;
using FestOS.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace FestOS.Modules.Riders.IntegrationTests;

/// <summary>
/// The Riders module, with Parties and Catalog it asks, on a migrated PostgreSQL database, with Identity for signing in: a plain host
/// for commands and data, and a web application wired like the Host for requests. Each test resets the
/// database first.
/// </summary>
public sealed class RidersFixture : IAsyncLifetime
{
    /// <summary>The password of the users the tests add.</summary>
    public const string Password = "doğru-at-pil-zımba";

    private readonly PostgresDatabase _database = new();
    private readonly Dictionary<string, string?> _settings = new(StringComparer.Ordinal);
    private IHost? _host;

    /// <summary>The clock of both hosts; starts on a microsecond-aligned Monday morning in Istanbul.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero));

    /// <summary>The user the plain host acts for.</summary>
    public FakeCurrentUser CurrentUser { get; } = new();

    /// <summary>The plain host's services.</summary>
    public IServiceProvider Services => _host!.Services;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        _settings[$"ConnectionStrings:{DatabaseConnections.ConnectionStringName}"] = _database.AdminConnectionString;
        _settings[$"Database:Passwords:{DatabaseRoles.Migrator}"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_audit"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_identity"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_inventory"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_catalog"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_parties"] = Guid.CreateVersion7().ToString("N");
        _settings["Database:Passwords:festos_riders"] = Guid.CreateVersion7().ToString("N");

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(_settings);
        builder.Services.AddLogging();
        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.Services.AddSingleton<ICurrentUser>(CurrentUser);
        builder.AddModules(Modules());
        _host = builder.Build();
        await _host.Services.GetRequiredService<DatabaseMigrator>().RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Starts a web application wired like the Host, served in memory; the caller disposes it.</summary>
    public async Task<WebApplication> StartWebApplicationAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(_settings);
        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.AddHttpPlatform();
        builder.AddRealtime();
        builder.AddModules(Modules());

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

    /// <summary>A client that sends requests like the front end, over HTTPS (testing §6).</summary>
    public static HttpClient CreateClient(WebApplication app) =>
        new(new BrowserLikeHandler { InnerHandler = app.GetTestServer().CreateHandler() })
        {
            BaseAddress = new Uri("https://localhost/"),
        };

    /// <summary>Adds a user with <see cref="Password"/> who has already set it.</summary>
    public async Task AddUserAsync(string email, Role role)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var user = User.Create(
            "Ayşe Kaya",
            email,
            [role],
            role == Role.WarehouseManager ? [Guid.CreateVersion7()] : [],
            scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(Password)
        );
        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Test data only: the state a user reaches by setting a password.
        await context
            .Users.Where(found => found.Id == user.Id)
            .ExecuteUpdateAsync(
                row => row.SetProperty(found => found.MustChangePassword, false),
                TestContext.Current.CancellationToken
            );
    }

    /// <summary>Empties the module tables and the change history.</summary>
    public Task ResetAsync() =>
        _database.ResetAsync(
            [
                AuditModuleDefinition.SchemaName,
                IdentityModuleDefinition.SchemaName,
                InventoryModuleDefinition.SchemaName,
                CatalogModuleDefinition.SchemaName,
                PartiesModuleDefinition.SchemaName,
                RidersModuleDefinition.SchemaName,
            ],
            TestContext.Current.CancellationToken
        );

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _host?.Dispose();
        await _database.DisposeAsync();
    }

    private static IModuleDefinition[] Modules() =>
        [
            new AuditModuleDefinition(),
            new IdentityModuleDefinition(),
            new InventoryModuleDefinition(),
            new CatalogModuleDefinition(),
            new PartiesModuleDefinition(),
            new RidersModuleDefinition(),
        ];
}
