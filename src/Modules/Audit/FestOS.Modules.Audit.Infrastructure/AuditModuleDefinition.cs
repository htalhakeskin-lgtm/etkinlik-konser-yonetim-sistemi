using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Audit.Infrastructure;

/// <summary>Registers the Audit module (05 §5.2).</summary>
public sealed class AuditModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Audit";

    /// <summary>The database schema.</summary>
    public const string SchemaName = AuditEntry.SchemaName;

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => [];

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<AuditDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(AuditModuleDefinition).Assembly);
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
