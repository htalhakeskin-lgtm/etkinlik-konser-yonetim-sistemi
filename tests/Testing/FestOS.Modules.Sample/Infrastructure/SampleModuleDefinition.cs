using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
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
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
