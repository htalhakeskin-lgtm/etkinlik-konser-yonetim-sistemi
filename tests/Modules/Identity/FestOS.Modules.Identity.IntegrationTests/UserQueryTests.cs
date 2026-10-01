using System.Globalization;
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

/// <summary>The users list and one user over HTTP (US-SYS-001, identity §7).</summary>
public sealed class UserQueryTests(IdentityFixture fixture) : IAsyncLifetime
{
    private const string Password = "doğru-at-pil-zımba";

    private WebApplication? _app;
    private string _passwordHash = "";

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        _passwordHash = fixture.Services.GetRequiredService<IPasswordHasher>().Hash(Password);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task List_SortsActiveUsersByNameInTurkishOrder_WithoutTheSystemUser()
    {
        await AddUserAsync("Zeynep Ak", "zeynep@example.com", Role.SystemAdministrator);
        await AddUserAsync("Çağlar Er", "caglar@example.com", Role.BookingManager);
        await AddUserAsync("Can Ok", "can@example.com", Role.TechnicalManager);
        UserId inactive = await AddUserAsync("Deniz Su", "deniz@example.com", Role.BookingManager);
        await DeactivateAsync(inactive);
        using HttpClient client = await SignedInAsync("zeynep@example.com");

        JsonElement page = await GetAsync(client, "/api/v1/users");

        Names(page).ShouldBe(["Can Ok", "Çağlar Er", "Zeynep Ak"]);
        page.GetProperty("totalCount").GetInt32().ShouldBe(3);
        page.GetProperty("items")[0].GetProperty("roles")[0].GetString().ShouldBe("technicalManager");
        page.GetProperty("items")[0].GetProperty("isActive").GetBoolean().ShouldBeTrue();
    }

    [Theory]
    [InlineData("isik", "Işık Yıldız")]
    [InlineData("IŞIK", "Işık Yıldız")]
    [InlineData("ipek@", "İpek Gül")]
    public async Task List_FindsByPartOfTheNameOrEmail_IgnoringCaseAndTurkishMarks(string q, string found)
    {
        await AddUserAsync("Zeynep Ak", "zeynep@example.com", Role.SystemAdministrator);
        await AddUserAsync("Işık Yıldız", "isik@example.com", Role.BookingManager);
        await AddUserAsync("İpek Gül", "ipek@example.com", Role.BookingManager);
        using HttpClient client = await SignedInAsync("zeynep@example.com");

        JsonElement page = await GetAsync(client, $"/api/v1/users?q={Uri.EscapeDataString(q)}");

        Names(page).ShouldBe([found]);
    }

    [Fact]
    public async Task List_FiltersByRoleAndStatus_AndPages()
    {
        await AddUserAsync("Zeynep Ak", "zeynep@example.com", Role.SystemAdministrator);
        await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        await AddUserAsync("Banu Can", "banu@example.com", Role.BookingManager);
        UserId inactive = await AddUserAsync("Cem Dal", "cem@example.com", Role.BookingManager);
        await DeactivateAsync(inactive);
        using HttpClient client = await SignedInAsync("zeynep@example.com");

        JsonElement firstPage = await GetAsync(client, "/api/v1/users?role=bookingManager&status=all&pageSize=2");
        JsonElement secondPage = await GetAsync(
            client,
            "/api/v1/users?role=bookingManager&status=all&pageSize=2&page=2"
        );
        JsonElement inactiveOnly = await GetAsync(client, "/api/v1/users?status=inactive&sort=-email");

        Names(firstPage).ShouldBe(["Ali Bal", "Banu Can"]);
        firstPage.GetProperty("totalCount").GetInt32().ShouldBe(3);
        Names(secondPage).ShouldBe(["Cem Dal"]);
        Names(inactiveOnly).ShouldBe(["Cem Dal"]);
        inactiveOnly.GetProperty("items")[0].GetProperty("isActive").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task List_RefusesASortFieldItDoesNotOffer()
    {
        await AddUserAsync("Zeynep Ak", "zeynep@example.com", Role.SystemAdministrator);
        using HttpClient client = await SignedInAsync("zeynep@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/users?sort=passwordHash", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        JsonElement error = (await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("errors")[
            0
        ];
        error.GetProperty("code").GetString().ShouldBe("unsupportedSort");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task List_NeedsThePermissionToViewUsers()
    {
        await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        using HttpClient client = await SignedInAsync("ali@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/users", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_ReturnsTheUserWithTheVersionAsETag()
    {
        await AddUserAsync("Zeynep Ak", "zeynep@example.com", Role.SystemAdministrator);
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.TechnicalManager);
        using HttpClient client = await SignedInAsync("zeynep@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri($"/api/v1/users/{user.Value}", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage missing = await client.GetAsync(
            new Uri($"/api/v1/users/{Guid.CreateVersion7()}", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        int version = body.GetProperty("version").GetInt32();
        response.Headers.ETag!.Tag.ShouldBe(string.Create(CultureInfo.InvariantCulture, $"\"{version}\""));
        body.GetProperty("email").GetString().ShouldBe("ali@example.com");
        body.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static List<string?> Names(JsonElement page) =>
        [.. page.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("fullName").GetString())];

    private static async Task<JsonElement> GetAsync(HttpClient client, string address)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private async Task<UserId> AddUserAsync(string fullName, string email, Role role)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var user = User.Create(fullName, email, [role], [], _passwordHash);
        context.Users.Add(user);
        await context.SaveChangesAsync(Cancellation);

        // Test data only: the state a user reaches by setting a password.
        await context
            .Users.Where(found => found.Id == user.Id)
            .ExecuteUpdateAsync(row => row.SetProperty(found => found.MustChangePassword, false), Cancellation);
        return user.Id;
    }

    // Test data only: deactivating arrives with the user commands.
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
