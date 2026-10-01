using System.Globalization;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.OpenApi;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Identity.Infrastructure.Users;
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
    return 0;
}

// Makes the first system administrator of a new installation (docs/modules/identity.md, ID-07).
if (args is ["create-admin", .. var adminArgs])
{
    HostApplicationBuilder adminBuilder = Host.CreateApplicationBuilder(adminArgs);
    adminBuilder.Services.AddSingleton<ICurrentUser, SystemCurrentUser>();
    adminBuilder.AddModules(Modules());
    using IHost adminHost = adminBuilder.Build();
#pragma warning disable RS0030 // The temporary password goes to the operator's console only, never to the logs.
    return await CreateAdminCommandLine.RunAsync(adminHost.Services, adminBuilder.Configuration, Console.Out);
#pragma warning restore RS0030
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
return 0;

// The only list of modules; a new module is added here (08 §5).
static IModuleDefinition[] Modules() => [new AuditModuleDefinition(), new IdentityModuleDefinition()];

/// <summary>The Host's entry point; public so the architecture tests can start it in memory (AT-09).</summary>
public partial class Program;
