using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.Modules.Identity.Application;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.ArchitectureTests;

/// <summary>The role matrix against the permission catalog of the running Host (security §3.2, ADR-0026).</summary>
public sealed class PermissionMatrixTests
{
    [Fact]
    public async Task RoleMatrixAndCatalog_HaveTheSamePermissions()
    {
        await using var host = new WebApplicationFactory<Program>();
        HashSet<string> catalog = new(
            host.Services.GetRequiredService<ModuleCatalog>().Permissions,
            StringComparer.Ordinal
        );
        HashSet<string> granted = new(
            RoleCatalog.Permissions.Values.SelectMany(permissions => permissions),
            StringComparer.Ordinal
        );

        catalog
            .Where(permission => !granted.Contains(permission))
            .ShouldBeEmpty("every permission belongs to at least one role");
        granted
            .Where(permission => !catalog.Contains(permission))
            .ShouldBeEmpty("every permission a role grants is defined by a module");
    }
}
