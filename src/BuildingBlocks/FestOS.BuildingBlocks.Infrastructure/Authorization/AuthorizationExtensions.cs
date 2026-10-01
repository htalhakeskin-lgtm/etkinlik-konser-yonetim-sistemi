using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Authorization;

/// <summary>Permission-based authorization of endpoints (security §3.3, ADR-0026).</summary>
public static class AuthorizationExtensions
{
    /// <summary>Registers authorization with one policy per permission of the module catalog.</summary>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        return services;
    }

    /// <summary>
    /// Requires the permission for the endpoint, e.g. <c>RequirePermission(IdentityPermissions.Users.View)</c>.
    /// Without a signed-in user the answer is 401, without the permission 403 (api §8.3).
    /// </summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return builder.RequireAuthorization(permission);
    }
}
