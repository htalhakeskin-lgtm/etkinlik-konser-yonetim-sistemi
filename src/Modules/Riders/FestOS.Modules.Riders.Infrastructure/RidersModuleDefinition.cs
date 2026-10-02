using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Riders.Application.Productions;
using FestOS.Modules.Riders.Application.Riders;
using FestOS.Modules.Riders.Contracts;
using FestOS.Modules.Riders.Infrastructure.Productions;
using FestOS.Modules.Riders.Infrastructure.Riders;
using FestOS.Modules.Riders.IntegrationEvents;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Riders.Infrastructure;

/// <summary>Registers the Riders module (05 §5.6, riders.md).</summary>
public sealed class RidersModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Riders";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "riders";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => RidersPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<RidersDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IProductionRepository).Assembly);
        builder.Services.AddHandlersFrom(typeof(RidersModuleDefinition).Assembly);
        builder.Services.AddScoped<IProductionRepository, ProductionRepository>();
        builder.Services.AddScoped<IRiderRepository, RiderRepository>();
        builder.Services.AddRealtimeGroup<RiderGroupPolicy>();
        builder.Services.AddResourceChange<RiderVersionCreatedIntegrationEvent>(created => new ResourceChange(
            "riders",
            created.RiderId,
            Version: null,
            [$"riders:{created.RiderId}"]
        ));
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ProductionEndpoints.Map(endpoints);
        RiderEndpoints.Map(endpoints);
    }
}
