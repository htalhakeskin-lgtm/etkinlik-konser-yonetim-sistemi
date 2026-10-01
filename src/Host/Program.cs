using System.Globalization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.OpenApi;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.ServiceDefaults;

// Server code runs with the invariant culture; text for people is formatted with tr-TR explicitly
// (docs/08-architecture.md §9).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// The same executable runs the commands of the release script (docs/09-environments-and-deployment.md §5).
if (args is ["migrate", .. var migrateArgs])
{
    HostApplicationBuilder migrateBuilder = Host.CreateApplicationBuilder(migrateArgs);
    migrateBuilder.AddModules(Modules());
    using IHost migrateHost = migrateBuilder.Build();
    await migrateHost.Services.GetRequiredService<DatabaseMigrator>().RunAsync(CancellationToken.None);
    return;
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddHttpPlatform();
builder.AddApiDocument();
builder.AddRealtime();
builder.AddModules(Modules());

WebApplication app = builder.Build();
app.UseHttpPlatform();

// Authentication (1.2) goes before this line: the antiforgery token is bound to the signed-in user.
app.UseCsrfProtection();
app.UseAuthorization();

// Development prepares the database on startup; other environments run "migrate" as a separate
// release step (docs/standards/database.md §16.2).
if (
    app.Environment.IsDevelopment()
    && app.Configuration.GetConnectionString(DatabaseConnections.ConnectionStringName) is not null
)
{
    await app.Services.GetRequiredService<DatabaseMigrator>().RunAsync(CancellationToken.None);
}

app.MapDefaultEndpoints();
app.MapAntiforgeryToken();
app.MapRealtime();
app.MapModules();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();

// The only list of modules; a new module is added here (08 §5).
static IModuleDefinition[] Modules() => [new AuditModuleDefinition()];

/// <summary>The Host's entry point; public so the architecture tests can start it in memory (AT-09).</summary>
public partial class Program;
