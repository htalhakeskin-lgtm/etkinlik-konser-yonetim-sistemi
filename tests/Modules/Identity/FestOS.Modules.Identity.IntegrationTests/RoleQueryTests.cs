using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.IntegrationTests;

/// <summary>The role and permission matrix over HTTP (US-SYS-003, identity §5.2).</summary>
public sealed class RoleQueryTests(IdentityFixture fixture) : IAsyncLifetime
{
    private const string Password = "doğru-at-pil-zımba";

    private WebApplication? _app;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Rule", "BR-SYS-004")]
    public async Task Matrix_GroupsThePermissionsByModule_WithTheRolesThatHoldThem()
    {
        await AddUserAsync("genel@example.com", Role.GeneralManager);
        using HttpClient client = await SignedInAsync("genel@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/roles", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement matrix = await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        matrix.GetProperty("roles").GetArrayLength().ShouldBe(5);
        JsonElement identity = matrix
            .GetProperty("modules")
            .EnumerateArray()
            .Single(module =>
                string.Equals(module.GetProperty("module").GetString(), "Identity", StringComparison.Ordinal)
            );
        var grants = identity
            .GetProperty("permissions")
            .EnumerateArray()
            .ToDictionary(
                grant => grant.GetProperty("code").GetString()!,
                grant => grant.GetProperty("roles").EnumerateArray().Select(role => role.GetString()!).ToArray(),
                StringComparer.Ordinal
            );
        grants["Identity.Users.View"].ShouldBe(["systemAdministrator", "generalManager"]);
        grants["Identity.Users.Create"].ShouldBe(["systemAdministrator"]);
        grants.Count.ShouldBe(6);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task Matrix_NeedsThePermissionToViewRoles()
    {
        await AddUserAsync("booking@example.com", Role.BookingManager);
        using HttpClient client = await SignedInAsync("booking@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/roles", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task AddUserAsync(string email, Role role)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var user = User.Create(
            "Ayşe Kaya",
            email,
            [role],
            [],
            scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(Password)
        );
        context.Users.Add(user);
        await context.SaveChangesAsync(Cancellation);

        // Test data only: the state a user reaches by setting a password.
        await context
            .Users.Where(found => found.Id == user.Id)
            .ExecuteUpdateAsync(row => row.SetProperty(found => found.MustChangePassword, false), Cancellation);
    }

    private async Task<HttpClient> SignedInAsync(string email)
    {
        HttpClient client = IdentityFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
