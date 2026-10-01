using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.UnitTests;

/// <summary>The role and permission matrix (security §3.2, ADR-0026).</summary>
public sealed class RoleCatalogTests
{
    [Fact]
    public void EveryRole_HasAnEntry() =>
        RoleCatalog.Permissions.Keys.ShouldBe(Enum.GetValues<Role>(), ignoreOrder: true);

    [Fact]
    [Trait("Rule", "BR-SYS-004")]
    public void GeneralManager_OnlyViews() =>
        RoleCatalog
            .Permissions[Role.GeneralManager]
            .ShouldAllBe(permission => permission.EndsWith(".View", StringComparison.Ordinal));

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public void PermissionsOfSeveralRoles_AreTheUnionOfTheirs()
    {
        IReadOnlySet<string> union = RoleCatalog.PermissionsOf([Role.GeneralManager, Role.SystemAdministrator]);

        union
            .SetEquals(
                RoleCatalog
                    .Permissions[Role.GeneralManager]
                    .Union(RoleCatalog.Permissions[Role.SystemAdministrator], StringComparer.Ordinal)
            )
            .ShouldBeTrue();
    }

    [Fact]
    public void SystemAdministrator_ManagesUsers() =>
        RoleCatalog.Permissions[Role.SystemAdministrator].IsSupersetOf(IdentityPermissions.All).ShouldBeTrue();
}
