using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace FestOS.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// How the Host registers a module (08 §5). Each module's Infrastructure project has one, named
/// <c>{Module}ModuleDefinition</c>, and the Host lists them explicitly; there is no assembly scanning.
/// </summary>
public interface IModuleDefinition
{
    /// <summary>The module name, e.g. <c>Booking</c>; also the OpenAPI tag of its endpoints (AT-14).</summary>
    string Name { get; }

    /// <summary>The module's database schema, e.g. <c>booking</c>; its role is <c>festos_{schema}</c>.</summary>
    string Schema { get; }

    /// <summary>The permissions the module defines, e.g. <c>Planning.Reservations.Confirm</c>.</summary>
    IReadOnlyCollection<string> Permissions { get; }

    /// <summary>
    /// Registers the database context, handlers, the synchronous contract implementation, event
    /// handlers, scheduled jobs and options.
    /// </summary>
    void RegisterServices(IHostApplicationBuilder builder);

    /// <summary>Maps the module's endpoints on a group under <c>/api/v1</c> tagged with <see cref="Name"/>.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
