using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Identity.Infrastructure.Sessions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FestOS.Modules.Identity.IntegrationTests;

/// <summary>Signing in and out over HTTP, with server-side sessions (identity §6, §7).</summary>
public sealed class SignInTests(IdentityFixture fixture) : IAsyncLifetime
{
    private const string Password = "doğru at pil zımba";

    private WebApplication? _app;
    private HttpClient? _client;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Client => _client!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        _client = IdentityFixture.CreateClient(_app);
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task Login_WithTheRightPassword_OpensASessionWithThePermissionsOfTheRoles()
    {
        await AddUserAsync("ayse@example.com", [Role.SystemAdministrator], mustChangePassword: false);

        using HttpResponseMessage login = await LoginAsync("Ayse@Example.com", Password);
        JsonElement me = await GetMeAsync();

        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.GetProperty("email").GetString().ShouldBe("ayse@example.com");
        me.GetProperty("roles").EnumerateArray().Select(role => role.GetString()).ShouldBe(["systemAdministrator"]);
        me.GetProperty("permissions")
            .EnumerateArray()
            .Any(permission => string.Equals(permission.GetString(), "Identity.Users.View", StringComparison.Ordinal))
            .ShouldBeTrue();
        (await SessionCountAsync()).ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public async Task Login_WithATemporaryPassword_GivesNoPermissionUntilANewPasswordIsSet()
    {
        await AddUserAsync("ayse@example.com", [Role.SystemAdministrator], mustChangePassword: true);

        await LoginAsync("ayse@example.com", Password);
        JsonElement me = await GetMeAsync();

        me.GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
        me.GetProperty("permissions").GetArrayLength().ShouldBe(0);
    }

    [Theory]
    [InlineData("ayse@example.com", "yanlış şifre ama uzun olan")]
    [InlineData("kimse@example.com", Password)]
    public async Task Login_WithAWrongEmailOrPassword_DoesNotSayWhichIsWrong(string email, string password)
    {
        await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);

        using HttpResponseMessage login = await LoginAsync(email, password);

        login.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await CodeOfAsync(login)).ShouldBe("invalidCredentials");
        (await SessionCountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Login_OfADeactivatedUser_IsRefused()
    {
        UserId user = await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        await DeactivateAsync(user);

        using HttpResponseMessage login = await LoginAsync("ayse@example.com", Password);

        login.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_EndsTheSessionOnTheServer()
    {
        await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        await LoginAsync("ayse@example.com", Password);

        using HttpResponseMessage logout = await Client.PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative),
            null,
            Cancellation
        );
        using HttpResponseMessage me = await Client.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation);

        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await SessionCountAsync()).ShouldBe(0);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-008")]
    public async Task Session_EndsAfterTwelveHoursWithoutARequest()
    {
        await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        await LoginAsync("ayse@example.com", Password);

        fixture.Time.Advance(TimeSpan.FromHours(12));
        using HttpResponseMessage me = await Client.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation);

        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-008")]
    public async Task Session_EndsTwentyFourHoursAfterSigningInEvenWhenUsed()
    {
        await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        await LoginAsync("ayse@example.com", Password);

        fixture.Time.Advance(TimeSpan.FromHours(11));
        (await Client.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation)).StatusCode.ShouldBe(
            HttpStatusCode.OK
        );
        fixture.Time.Advance(TimeSpan.FromHours(11));
        (await Client.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation)).StatusCode.ShouldBe(
            HttpStatusCode.OK
        );
        fixture.Time.Advance(TimeSpan.FromHours(2));
        using HttpResponseMessage me = await Client.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation);

        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ReplacesAHashMadeWithOlderSettings()
    {
        var older = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { IterationCount = 100_000 }));
        UserId user = await AddUserAsync(
            "ayse@example.com",
            [Role.BookingManager],
            mustChangePassword: false,
            older.HashPassword(null!, Password)
        );

        await LoginAsync("ayse@example.com", Password);

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        User stored = await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.SingleAsync(found => found.Id == user, Cancellation);
        scope
            .ServiceProvider.GetRequiredService<IPasswordHasher>()
            .Verify(stored.PasswordHash, Password, out bool needsRehash)
            .ShouldBeTrue();
        needsRehash.ShouldBeFalse();
    }

    private async Task<UserId> AddUserAsync(
        string email,
        Role[] roles,
        bool mustChangePassword,
        string? passwordHash = null
    )
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var user = User.Create(
            "Ayşe Kaya",
            email,
            roles,
            [],
            passwordHash ?? scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(Password)
        );
        context.Users.Add(user);
        await context.SaveChangesAsync(Cancellation);
        if (!mustChangePassword)
        {
            await ClearMustChangePasswordAsync(user.Id);
        }

        return user.Id;
    }

    // Test data only: states the module reaches through features of later PRs.
    private async Task ClearMustChangePasswordAsync(UserId user)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.Where(found => found.Id == user)
            .ExecuteUpdateAsync(row => row.SetProperty(found => found.MustChangePassword, false), Cancellation);
    }

    private async Task DeactivateAsync(UserId user)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.Where(found => found.Id == user)
            .ExecuteUpdateAsync(
                row => row.SetProperty(found => found.DeactivatedAt, fixture.Time.GetUtcNow()),
                Cancellation
            );
    }

    private async Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        await Client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password },
            Cancellation
        );

    private async Task<JsonElement> GetMeAsync()
    {
        using HttpResponseMessage response = await Client.GetAsync(
            new Uri("/api/v1/me", UriKind.Relative),
            Cancellation
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private async Task<int> SessionCountAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Set<Session>()
            .CountAsync(Cancellation);
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return problem.RootElement.GetProperty("code").GetString();
    }
}
