using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Application.Roles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Identity.Infrastructure.Roles;

/// <summary>The role and permission matrix (identity §7).</summary>
internal static class RoleEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet("/roles", ListRolesAsync)
            .RequirePermission(IdentityPermissions.ViewRoles)
            .WithName("ListRoles")
            .WithSummary("Lists the roles and the permissions each holds, grouped by module.");

    private static async Task<Ok<RoleMatrix>> ListRolesAsync(
        IQueryHandler<ListRolesQuery, RoleMatrix> listRoles,
        CancellationToken cancellationToken
    ) => TypedResults.Ok(await listRoles.HandleAsync(new ListRolesQuery(), cancellationToken));
}
