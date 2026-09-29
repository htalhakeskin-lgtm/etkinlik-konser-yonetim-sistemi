using System.Globalization;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.ServiceDefaults;

// Server code runs with the invariant culture; text for people is formatted with tr-TR explicitly
// (docs/08-architecture.md §9).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

// The only list of modules; a new module is added here (08 §5).
builder.AddModules();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapModules();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();
