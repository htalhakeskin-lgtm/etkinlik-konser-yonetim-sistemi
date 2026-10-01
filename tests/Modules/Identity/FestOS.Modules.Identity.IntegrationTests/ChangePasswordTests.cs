using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Identity.Infrastructure.Sessions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.IntegrationTests;

/// <summary>Setting a new password over HTTP (US-SYS-011, identity §6).</summary>
public sealed class ChangePasswordTests(IdentityFixture fixture) : IAsyncLifetime
{
    private const string Password = "doğru-at-pil-zımba";
    private const string NewPassword = "mavi-kalem-uzun-yol";

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
    [Trait("Rule", "BR-SYS-006")]
    public async Task FromATemporaryPassword_NoCurrentPasswordIsAsked_AndThisSessionGetsThePermissions()
    {
        await AddUserAsync(mustChangePassword: true);
        using HttpClient client = await SignedInClientAsync(Password);

        using HttpResponseMessage change = await ChangeAsync(client, currentPassword: null, NewPassword);
        JsonElement changed = await change.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        JsonElement me = await GetMeAsync(client);

        change.StatusCode.ShouldBe(HttpStatusCode.OK);
        changed.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
        me.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
        Session session = (await SessionsAsync()).ShouldHaveSingleItem();
        session.MustChangePassword.ShouldBeFalse();
        session.Permissions.ShouldContain(permission =>
            string.Equals(permission, "Identity.Users.View", StringComparison.Ordinal)
        );
    }

    [Fact]
    [Trait("Rule", "BR-SYS-007")]
    public async Task NewPassword_EndsTheUsersOtherSessions_AndReplacesTheOldPassword()
    {
        await AddUserAsync(mustChangePassword: false);
        using HttpClient client = await SignedInClientAsync(Password);
        using HttpClient otherBrowser = await SignedInClientAsync(Password);
        await GetMeAsync(otherBrowser);

        using HttpResponseMessage change = await ChangeAsync(client, Password, NewPassword);
        using HttpResponseMessage otherAfter = await otherBrowser.GetAsync(
            new Uri("/api/v1/me", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage oldPassword = await LoginAsync(NewClient(), Password);
        using HttpResponseMessage newPassword = await LoginAsync(
            NewClient(),
            NewPassword.Normalize(System.Text.NormalizationForm.FormD)
        );

        change.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetMeAsync(client)).GetProperty("email").GetString().ShouldBe("ayse@example.com");
        otherAfter.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        oldPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        newPassword.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithAWrongCurrentPassword_IsRefusedOnThatField()
    {
        await AddUserAsync(mustChangePassword: false);
        using HttpClient client = await SignedInClientAsync(Password);

        using HttpResponseMessage change = await ChangeAsync(client, "yanlış-şifre-yazıldı", NewPassword);

        change.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        JsonElement error = (await ProblemAsync(change)).GetProperty("errors")[0];
        error.GetProperty("pointer").GetString().ShouldBe("/currentPassword");
        error.GetProperty("code").GetString().ShouldBe(ChangeMyPasswordHandler.IncorrectPassword);
    }

    [Theory]
    [Trait("Rule", "BR-SYS-007")]
    [InlineData("kısa-şifre", "tooShort")]
    [InlineData("boşluklu şifre olmaz", "whitespace")]
    [InlineData("1QAZ2wsx3edc4rfv", "common")]
    [InlineData("ayse@example.com-ile", "containsEmail")]
    [InlineData("benim-festos-şifrem", "containsProductName")]
    [InlineData(Password, "sameAsCurrent")]
    public async Task BreakingThePolicy_SaysWhichRuleAndKeepsTheOldPassword(string newPassword, string reason)
    {
        await AddUserAsync(mustChangePassword: false);
        using HttpClient client = await SignedInClientAsync(Password);

        using HttpResponseMessage change = await ChangeAsync(client, Password, newPassword);

        change.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await ProblemAsync(change);
        problem.GetProperty("code").GetString().ShouldBe("BR-SYS-007");
        problem.GetProperty("params").GetProperty("reason").GetString().ShouldBe(reason);
        problem.GetProperty("params").GetProperty("minLength").GetInt32().ShouldBe(15);
        (await SessionsAsync()).Count.ShouldBe(1);
    }

    private async Task AddUserAsync(bool mustChangePassword)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var user = User.Create(
            "Ayşe Kaya",
            "ayse@example.com",
            [Role.SystemAdministrator],
            [],
            scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(Password)
        );
        context.Users.Add(user);
        await context.SaveChangesAsync(Cancellation);
        if (!mustChangePassword)
        {
            // Test data only: the state a user reaches by setting a password.
            await context
                .Users.Where(found => found.Id == user.Id)
                .ExecuteUpdateAsync(row => row.SetProperty(found => found.MustChangePassword, false), Cancellation);
        }
    }

    private HttpClient NewClient() => IdentityFixture.CreateClient(_app!);

    private async Task<HttpClient> SignedInClientAsync(string password)
    {
        HttpClient client = NewClient();
        using HttpResponseMessage login = await LoginAsync(client, password);
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string password) =>
        await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email = "ayse@example.com", password },
            Cancellation
        );

    private static async Task<HttpResponseMessage> ChangeAsync(
        HttpClient client,
        string? currentPassword,
        string newPassword
    ) =>
        await client.PostAsJsonAsync(
            new Uri("/api/v1/me/password", UriKind.Relative),
            new { currentPassword, newPassword },
            Cancellation
        );

    private static async Task<JsonElement> GetMeAsync(HttpClient client)
    {
        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/me", UriKind.Relative),
            Cancellation
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private static async Task<JsonElement> ProblemAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);

    private async Task<List<Session>> SessionsAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Set<Session>()
            .AsNoTracking()
            .ToListAsync(Cancellation);
    }
}
