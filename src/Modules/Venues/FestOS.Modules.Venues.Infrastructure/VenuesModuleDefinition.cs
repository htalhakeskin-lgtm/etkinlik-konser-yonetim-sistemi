using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Contracts;
using FestOS.Modules.Venues.Infrastructure.Venues;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Venues.Infrastructure;

/// <summary>Registers the Venues module (05 §5.5, venues.md).</summary>
public sealed class VenuesModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Venues";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "venues";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => VenuesPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<VenuesDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IVenueRepository).Assembly);
        builder.Services.AddHandlersFrom(typeof(VenuesModuleDefinition).Assembly);
        builder.Services.AddScoped<IVenueRepository, VenueRepository>();
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => VenueEndpoints.Map(endpoints);
}
