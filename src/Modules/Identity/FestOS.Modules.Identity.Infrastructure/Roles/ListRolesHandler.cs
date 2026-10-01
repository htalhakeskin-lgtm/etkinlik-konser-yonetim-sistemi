using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Application.Roles;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Infrastructure.Roles;

/// <summary>
/// Reads the matrix from its only definition, <see cref="RoleCatalog"/>, and the permissions from the
/// modules the Host runs (identity §5.2), so the screen never drifts from what the server checks.
/// </summary>
internal sealed class ListRolesHandler(ModuleCatalog catalog) : IQueryHandler<ListRolesQuery, RoleMatrix>
{
    public Task<RoleMatrix> HandleAsync(ListRolesQuery query, CancellationToken cancellationToken)
    {
        Role[] roles = Enum.GetValues<Role>();
        List<ModulePermissions> modules =
        [
            .. catalog
                .Modules.Where(module => module.Permissions.Count > 0)
                .Select(module => new ModulePermissions(
                    module.Name,
                    [
                        .. module.Permissions.Select(code => new PermissionGrant(
                            code,
                            [.. roles.Where(role => RoleCatalog.Permissions[role].Contains(code))]
                        )),
                    ]
                )),
        ];
        return Task.FromResult(new RoleMatrix(roles, modules));
    }
}
