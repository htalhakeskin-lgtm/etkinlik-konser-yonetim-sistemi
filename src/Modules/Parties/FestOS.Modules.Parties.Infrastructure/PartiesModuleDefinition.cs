using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Parties.Application.Parties;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Parties.Infrastructure.Parties;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Parties.Infrastructure;

/// <summary>Registers the Parties module (05 §5.3, parties.md).</summary>
public sealed class PartiesModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Parties";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "parties";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => PartiesPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<PartiesDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IPartyRepository).Assembly);
        builder.Services.AddHandlersFrom(typeof(PartiesModuleDefinition).Assembly);
        builder.Services.AddScoped<IPartyRepository, PartyRepository>();
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => PartyEndpoints.Map(endpoints);
}
