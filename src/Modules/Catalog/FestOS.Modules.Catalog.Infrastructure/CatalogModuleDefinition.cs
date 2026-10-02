using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Catalog.Infrastructure.Categories;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Catalog.Infrastructure;

/// <summary>Registers the Catalog module (05 §5.4, catalog.md).</summary>
public sealed class CatalogModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Catalog";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "catalog";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => CatalogPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<CatalogDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IEquipmentCategoryRepository).Assembly);
        builder.Services.AddHandlersFrom(typeof(CatalogModuleDefinition).Assembly);
        builder.Services.AddScoped<IEquipmentCategoryRepository, EquipmentCategoryRepository>();
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => EquipmentCategoryEndpoints.Map(endpoints);
}
