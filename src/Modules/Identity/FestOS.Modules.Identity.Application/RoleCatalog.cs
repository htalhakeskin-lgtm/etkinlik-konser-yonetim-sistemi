using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.Contracts;

namespace FestOS.Modules.Identity.Application;

/// <summary>
/// The role and permission matrix of S1, the only definition of it (security §3.2, ADR-0026). The
/// matrix screen and the tests read it; every module step adds its permissions here.
/// </summary>
public static class RoleCatalog
{
    // Audit's code, written out: Identity references no Audit project (08 §3.3). The architecture test that
    // every granted code is in the catalog keeps the two in step.
    private const string ViewAuditEntries = "Audit.Entries.View";

    // The codes of the step 1.3 modules, written out for the same reason (parties MD-04).
    private const string ViewParties = "Parties.Parties.View";

    private const string ViewCategories = "Catalog.Categories.View";

    private static readonly string[] ManageCategories =
    [
        ViewCategories,
        "Catalog.Categories.Create",
        "Catalog.Categories.Edit",
        "Catalog.Categories.Deactivate",
    ];

    private static readonly string[] ManageParties =
    [
        ViewParties,
        "Parties.Parties.Create",
        "Parties.Parties.Edit",
        "Parties.Parties.Deactivate",
    ];

    /// <summary>The permissions of each role.</summary>
    public static IReadOnlyDictionary<Role, IReadOnlySet<string>> Permissions { get; } =
        new Dictionary<Role, IReadOnlySet<string>>
        {
            [Role.SystemAdministrator] = Set([
                .. IdentityPermissions.All,
                .. InventoryPermissions.All,
                ViewAuditEntries,
            ]),
            [Role.BookingManager] = Set([InventoryPermissions.ViewWarehouses, .. ManageParties, ViewCategories]),
            [Role.TechnicalManager] = Set([InventoryPermissions.ViewWarehouses, ViewParties, .. ManageCategories]),
            [Role.WarehouseManager] = Set([InventoryPermissions.ViewWarehouses, ViewCategories]),
            [Role.GeneralManager] = Set([
                IdentityPermissions.ViewUsers,
                IdentityPermissions.ViewRoles,
                InventoryPermissions.ViewWarehouses,
                ViewParties,
                ViewCategories,
                ViewAuditEntries,
            ]),
        };

    /// <summary>The permissions of a user with these roles: the union of theirs (BR-SYS-002).</summary>
    public static IReadOnlySet<string> PermissionsOf(IEnumerable<Role> roles) =>
        roles.SelectMany(role => Permissions[role]).ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> Set(IEnumerable<string> permissions) => new(permissions, StringComparer.Ordinal);
}
