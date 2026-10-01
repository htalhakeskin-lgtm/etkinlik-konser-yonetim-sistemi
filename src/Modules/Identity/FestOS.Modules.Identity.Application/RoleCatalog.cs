using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.Contracts;

namespace FestOS.Modules.Identity.Application;

/// <summary>
/// The role and permission matrix of S1, the only definition of it (security §3.2, ADR-0026). The
/// matrix screen and the tests read it; every module step adds its permissions here.
/// </summary>
public static class RoleCatalog
{
    /// <summary>The permissions of each role.</summary>
    public static IReadOnlyDictionary<Role, IReadOnlySet<string>> Permissions { get; } =
        new Dictionary<Role, IReadOnlySet<string>>
        {
            [Role.SystemAdministrator] = Set([.. IdentityPermissions.All, .. InventoryPermissions.All]),
            [Role.BookingManager] = Set([InventoryPermissions.ViewWarehouses]),
            [Role.TechnicalManager] = Set([InventoryPermissions.ViewWarehouses]),
            [Role.WarehouseManager] = Set([InventoryPermissions.ViewWarehouses]),
            [Role.GeneralManager] = Set([
                IdentityPermissions.ViewUsers,
                IdentityPermissions.ViewRoles,
                InventoryPermissions.ViewWarehouses,
            ]),
        };

    /// <summary>The permissions of a user with these roles: the union of theirs (BR-SYS-002).</summary>
    public static IReadOnlySet<string> PermissionsOf(IEnumerable<Role> roles) =>
        roles.SelectMany(role => Permissions[role]).ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> Set(IEnumerable<string> permissions) => new(permissions, StringComparer.Ordinal);
}
