using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Identity.Domain;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.Contracts;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>
/// Checks with Inventory that a warehouse manager's warehouses exist and are active (BR-SYS-014, inventory §6);
/// other roles keep no warehouses, so theirs are not checked.
/// </summary>
internal static class WarehouseAssignments
{
    public static async Task EnsureActiveAsync(
        this IWarehouseDirectory warehouses,
        IReadOnlyList<Role> roles,
        IReadOnlyList<Guid> warehouseIds,
        CancellationToken cancellationToken
    )
    {
        if (!roles.Contains(Role.WarehouseManager) || warehouseIds.Count == 0)
        {
            return;
        }

        IReadOnlySet<Guid> active = await warehouses.FindActiveAsync([.. warehouseIds.Distinct()], cancellationToken);
        List<Guid> refused = [.. warehouseIds.Distinct().Where(id => !active.Contains(id))];
        if (refused.Count > 0)
        {
            throw new BusinessRuleViolationException(
                IdentityRuleCodes.WarehouseManagerHasWarehouse,
                "A warehouse manager's warehouses must exist and be active.",
                parameters: new Dictionary<string, object?>(StringComparer.Ordinal) { ["warehouseIds"] = refused }
            );
        }
    }
}
