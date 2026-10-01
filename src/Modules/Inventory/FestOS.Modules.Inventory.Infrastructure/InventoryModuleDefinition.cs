using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Contracts;
using FestOS.Modules.Inventory.Infrastructure.Warehouses;
using FestOS.Modules.Inventory.IntegrationEvents;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Inventory.Infrastructure;

/// <summary>Registers the Inventory module (05 §5, inventory.md); in step 1.2 it holds the warehouses.</summary>
public sealed class InventoryModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Inventory";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "inventory";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => InventoryPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<InventoryDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IWarehouseRepository).Assembly);
        builder.Services.AddHandlersFrom(typeof(InventoryModuleDefinition).Assembly);
        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        builder.Services.AddScoped<IWarehouseDirectory, WarehouseDirectory>();
        builder.Services.AddRealtimeGroup<WarehouseGroupPolicy>();
        builder.Services.AddResourceChange<WarehouseDeactivatedIntegrationEvent>(deactivated => new ResourceChange(
            "warehouses",
            deactivated.WarehouseId,
            Version: null,
            ["warehouses", $"warehouses:{deactivated.WarehouseId}"]
        ));
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => WarehouseEndpoints.Map(endpoints);
}
