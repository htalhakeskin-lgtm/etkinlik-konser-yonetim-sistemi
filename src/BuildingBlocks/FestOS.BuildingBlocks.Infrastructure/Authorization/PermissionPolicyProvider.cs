using FestOS.BuildingBlocks.Infrastructure.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Authorization;

/// <summary>
/// Turns every permission code of the module catalog into an authorization policy of the same name:
/// a signed-in user with that permission claim (security §3.3). Policies are not registered one by one.
/// A code outside the catalog has no policy, so a misspelled permission fails instead of passing.
/// </summary>
internal sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options, ModuleCatalog modules)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);
    private readonly HashSet<string> _permissions = new(modules.Permissions, StringComparer.Ordinal);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) =>
        _permissions.Contains(policyName)
            ? Task.FromResult<AuthorizationPolicy?>(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaims.Type, policyName)
                    .Build()
            )
            : _fallback.GetPolicyAsync(policyName);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}
