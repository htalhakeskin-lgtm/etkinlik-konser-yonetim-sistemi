using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using FestOS.Modules.Sample.Api;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.IntegrationEvents;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Sample.Infrastructure;

/// <summary>Registers the sample module in test hosts.</summary>
public sealed class SampleModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Sample";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "sample";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => [];

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<SampleDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(SampleModuleDefinition).Assembly);
        builder.Services.TryAddSingleton<SampleListenerProbe>();
        builder.Services.AddRealtimeGroup<SampleItemGroupPolicy>();
        builder.Services.AddResourceChange<SampleItemUsedIntegrationEvent>(used => new ResourceChange(
            "sample-items",
            used.SampleItemId,
            Version: null,
            ["sample-items", $"sample-items:{used.SampleItemId}"]
        ));
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => SampleEndpoints.Map(endpoints);
}
