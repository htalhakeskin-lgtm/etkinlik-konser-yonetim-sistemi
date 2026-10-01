using FestOS.BuildingBlocks.Infrastructure.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.ArchitectureTests;

/// <summary>Rules checked on the endpoints of the running Host (docs/08-architecture.md §12.2).</summary>
public sealed class EndpointAuthorizationTests
{
    [Fact]
    [Trait("ArchitectureRule", "AT-09")]
    public async Task ApiAndHubEndpoints_RequireAPermissionOrAreExplicitlyAnonymous()
    {
        await using var host = new WebApplicationFactory<Program>();
        IServiceProvider services = host.Services;
        HashSet<string> permissions = new(
            services.GetRequiredService<ModuleCatalog>().Permissions,
            StringComparer.Ordinal
        );
        List<string> violations = [];

        foreach (
            RouteEndpoint endpoint in services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints.OfType<RouteEndpoint>()
        )
        {
            string route = "/" + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/');
            if (
                !route.StartsWith("/api/", StringComparison.Ordinal)
                && !route.StartsWith("/hubs/", StringComparison.Ordinal)
            )
            {
                continue;
            }

            bool isAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
            IAuthorizeData[] authorization = [.. endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()];
            string[] unknown =
            [
                .. authorization
                    .Select(data => data.Policy)
                    .OfType<string>()
                    .Where(policy => !permissions.Contains(policy)),
            ];

            if (!isAnonymous && authorization.Length == 0)
            {
                violations.Add($"{route}: requires no permission and is not marked anonymous");
            }

            violations.AddRange(unknown.Select(policy => $"{route}: {policy} is not in the permission catalog"));
        }

        violations.ShouldBeEmpty();
    }
}
