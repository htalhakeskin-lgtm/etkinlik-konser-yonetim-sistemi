using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Identity.Infrastructure.Authentication;
using FestOS.Modules.Identity.Infrastructure.Sessions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SignInResult = FestOS.Modules.Identity.Application.Authentication.SignInResult;

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

    [Fact]
    [Trait("Rule", "BR-SYS-005")]
    public async Task WrongPasswords_LockTheAccountAtTheFifthInARow_EvenForTheRightPassword()
    {
        await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        DateTimeOffset lockedUntil = fixture.Time.GetUtcNow().AddMinutes(15);

        SignInResult[] wrong = [.. await RepeatAsync(5, () => SignInAsync("ayse@example.com", "yanlış şifre"))];
        SignInResult right = await SignInAsync("ayse@example.com", Password);
        fixture.Time.Advance(TimeSpan.FromMinutes(15));
        SignInResult afterTheLock = await SignInAsync("ayse@example.com", Password);

        wrong[..4].ShouldAllBe(result => result == SignInResult.Refused);
        wrong[4].LockedUntil.ShouldBe(lockedUntil);
        right.LockedUntil.ShouldBe(lockedUntil);
        afterTheLock.User.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-005")]
    public async Task SuccessfulLogin_StartsTheCountOfWrongPasswordsAgain()
    {
        await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);

        await RepeatAsync(4, () => SignInAsync("ayse@example.com", "yanlış şifre"));
        await SignInAsync("ayse@example.com", Password);
        SignInResult[] wrong = [.. await RepeatAsync(4, () => SignInAsync("ayse@example.com", "yanlış şifre"))];

        wrong.ShouldAllBe(result => result == SignInResult.Refused);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-005")]
    public async Task Login_OfALockedAccount_Answers401WithWhenTheLockEnds()
    {
        UserId user = await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        DateTimeOffset lockedUntil = fixture.Time.GetUtcNow().AddMinutes(10);
        await LockAsync(user, lockedUntil);

        using HttpResponseMessage login = await LoginAsync("ayse@example.com", Password);

        login.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var problem = JsonDocument.Parse(await login.Content.ReadAsStringAsync(Cancellation));
        problem.RootElement.GetProperty("code").GetString().ShouldBe("BR-SYS-005");
        problem.RootElement.GetProperty("params").GetProperty("lockedUntil").GetDateTimeOffset().ShouldBe(lockedUntil);
        (await SessionCountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task EveryAttempt_IsRecordedWithTheEmailTheUserAndTheOutcome()
    {
        UserId user = await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);

        (await LoginAsync("Kimse@Example.com", Password)).Dispose();
        (await LoginAsync("ayse@example.com", "yanlış şifre")).Dispose();
        (await LoginAsync("AYSE@example.com", Password)).Dispose();

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        var attempts = await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Set<LoginAttempt>()
            .Select(attempt => new
            {
                attempt.Email,
                attempt.UserId,
                attempt.Succeeded,
            })
            .ToListAsync(Cancellation);
        attempts.ShouldBe(
            [
                new
                {
                    Email = "kimse@example.com",
                    UserId = (UserId?)null,
                    Succeeded = false,
                },
                new
                {
                    Email = "ayse@example.com",
                    UserId = (UserId?)user,
                    Succeeded = false,
                },
                new
                {
                    Email = "ayse@example.com",
                    UserId = (UserId?)user,
                    Succeeded = true,
                },
            ],
            ignoreOrder: true
        );
    }

    [Fact]
    public async Task Login_MoreThanFiveTimesAMinuteForOneEmail_IsRefusedWithWhenToTryAgain()
    {
        HttpResponseMessage[] allowed = [.. await RepeatAsync(5, () => LoginAsync("kimse@example.com", Password))];
        using HttpResponseMessage sixth = await LoginAsync("kimse@example.com", Password);
        using HttpResponseMessage otherEmail = await LoginAsync("baska@example.com", Password);

        allowed.ShouldAllBe(response => response.StatusCode == HttpStatusCode.Unauthorized);
        sixth.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        (await CodeOfAsync(sixth)).ShouldBe("rateLimited");
        sixth.Headers.RetryAfter!.Delta!.Value.ShouldBeInRange(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));
        otherEmail.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        Array.ForEach(allowed, response => response.Dispose());
    }

    [Fact]
    public async Task Login_MoreThanTenTimesAMinuteFromOneAddress_IsRefused()
    {
        HttpResponseMessage[] allowed =
        [
            .. await RepeatAsync(
                10,
                attempt =>
                    LoginAsync(string.Create(CultureInfo.InvariantCulture, $"kimse{attempt}@example.com"), Password)
            ),
        ];
        using HttpResponseMessage eleventh = await LoginAsync("kimse10@example.com", Password);

        allowed.ShouldAllBe(response => response.StatusCode == HttpStatusCode.Unauthorized);
        eleventh.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        Array.ForEach(allowed, response => response.Dispose());
    }

    [Fact]
    [Trait("Rule", "BR-SYS-008")]
    public async Task SessionCleanup_DeletesTheEndedSessionsOnly()
    {
        UserId user = await AddUserAsync("ayse@example.com", [Role.BookingManager], mustChangePassword: false);
        DateTimeOffset now = fixture.Time.GetUtcNow();
        Guid live = await AddSessionAsync(user, lastSeenAt: now.AddHours(-11), expiresAt: now.AddHours(1));
        await AddSessionAsync(user, lastSeenAt: now.AddHours(-12), expiresAt: now.AddHours(1));
        await AddSessionAsync(user, lastSeenAt: now, expiresAt: now);

        await RunJobAsync("sessions-cleanup");

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        (
            await scope
                .ServiceProvider.GetRequiredService<IdentityDbContext>()
                .Set<Session>()
                .Select(session => session.Id)
                .ToListAsync(Cancellation)
        ).ShouldBe([live]);
    }

    [Fact]
    public async Task LoginAttemptCleanup_DeletesAttemptsOlderThanNinetyDays()
    {
        DateTimeOffset now = fixture.Time.GetUtcNow();
        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            context.AddRange(
                new LoginAttempt
                {
                    Id = Guid.CreateVersion7(),
                    Email = "eski@example.com",
                    OccurredAt = now.AddDays(-91),
                },
                new LoginAttempt
                {
                    Id = Guid.CreateVersion7(),
                    Email = "yeni@example.com",
                    OccurredAt = now.AddDays(-89),
                }
            );
            await context.SaveChangesAsync(Cancellation);
        }

        await RunJobAsync("login-attempts-cleanup");

        await using AsyncServiceScope check = fixture.Services.CreateAsyncScope();
        (
            await check
                .ServiceProvider.GetRequiredService<IdentityDbContext>()
                .Set<LoginAttempt>()
                .Select(attempt => attempt.Email)
                .ToListAsync(Cancellation)
        ).ShouldBe(["yeni@example.com"]);
    }

    private async Task<SignInResult> SignInAsync(string email, string password)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<SignInCommand, SignInResult>>()
            .HandleAsync(new SignInCommand(email, password), Cancellation);
    }

    // One after the other, since the order of the attempts is what is tested.
    private static async Task<List<T>> RepeatAsync<T>(int count, Func<int, Task<T>> attempt)
    {
        var results = new List<T>(count);
        for (int index = 0; index < count; index++)
        {
            results.Add(await attempt(index));
        }

        return results;
    }

    private static Task<List<T>> RepeatAsync<T>(int count, Func<Task<T>> attempt) => RepeatAsync(count, _ => attempt());

    private async Task<Guid> AddSessionAsync(UserId user, DateTimeOffset lastSeenAt, DateTimeOffset expiresAt)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var session = new Session
        {
            Id = Guid.CreateVersion7(),
            UserId = user,
            KeyHash = Convert.ToHexStringLower(Guid.CreateVersion7().ToByteArray()).PadRight(64, '0'),
            CreatedAt = expiresAt.AddHours(-24),
            LastSeenAt = lastSeenAt,
            ExpiresAt = expiresAt,
            Permissions = [],
            WarehouseIds = [],
        };
        context.Add(session);
        await context.SaveChangesAsync(Cancellation);
        return session.Id;
    }

    private async Task RunJobAsync(string name)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        await scope
            .ServiceProvider.GetServices<IScheduledJob>()
            .Single(job => string.Equals(job.Name, name, StringComparison.Ordinal))
            .RunAsync(scope.ServiceProvider, Cancellation);
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

    private async Task LockAsync(UserId user, DateTimeOffset until)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.Where(found => found.Id == user)
            .ExecuteUpdateAsync(row => row.SetProperty(found => found.LockedUntil, until), Cancellation);
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
