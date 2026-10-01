using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Identity.Application;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Identity.Infrastructure;

/// <summary>Registers the Identity module (05 §5.1, identity.md).</summary>
public sealed class IdentityModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Identity";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "identity";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => IdentityPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<IdentityDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IdentityPermissions).Assembly);
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
