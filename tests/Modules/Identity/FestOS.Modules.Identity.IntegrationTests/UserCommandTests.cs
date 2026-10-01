using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Domain.Warehouses;
using FestOS.Modules.Inventory.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.IntegrationTests;

/// <summary>Creating and managing users over HTTP (US-SYS-001, US-SYS-002, identity §7).</summary>
public sealed class UserCommandTests(IdentityFixture fixture) : IAsyncLifetime
{
    private const string Password = "doğru-at-pil-zımba";
    private const string AdminEmail = "zeynep@example.com";
    private static readonly string[] BookingManagerRole = ["bookingManager"];

    private WebApplication? _app;
    private string _passwordHash = "";
    private HttpClient? _admin;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Admin => _admin!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        _passwordHash = fixture.Services.GetRequiredService<IPasswordHasher>().Hash(Password);
        await AddUserAsync("Zeynep Ak", AdminEmail, Role.SystemAdministrator);
        _admin = await SignedInAsync(AdminEmail, Password);
    }

    public async ValueTask DisposeAsync()
    {
        _admin?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public async Task Create_GivesATemporaryPasswordOnce_ThatMustBeReplacedAtTheFirstSignIn()
    {
        using HttpResponseMessage created = await Admin.PostAsJsonAsync(
            new Uri("/api/v1/users", UriKind.Relative),
            new
            {
                fullName = "Ali Bal",
                email = "Ali@Example.com",
                roles = BookingManagerRole,
                warehouseIds = Array.Empty<Guid>(),
            },
            Cancellation
        );

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        JsonElement body = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        string temporaryPassword = body.GetProperty("temporaryPassword").GetString()!;
        created.Headers.Location!.ToString().ShouldBe($"/api/v1/users/{body.GetProperty("id").GetString()}");
        using HttpClient newUser = await SignedInAsync("ali@example.com", temporaryPassword);
        (await GetMeAsync(newUser)).GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-015")]
    public async Task Create_RefusesTheEmailOfAnotherUser_EvenADeactivatedOne()
    {
        UserId other = await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        await DeactivateDirectlyAsync(other);

        using HttpResponseMessage created = await CreateAsync("ALI@example.com", ["technicalManager"], []);

        created.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(created)).ShouldBe("BR-SYS-015");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-014")]
    public async Task Create_RefusesAWarehouseManagerWithoutAnActiveWarehouse()
    {
        Guid active = await AddWarehouseAsync("Merkez Depo");
        Guid inactive = await AddWarehouseAsync("Kuzey Depo", deactivated: true);

        using HttpResponseMessage withNone = await CreateAsync("ali@example.com", ["warehouseManager"], []);
        using HttpResponseMessage withInactive = await CreateAsync("ali@example.com", ["warehouseManager"], [inactive]);
        using HttpResponseMessage withUnknown = await CreateAsync(
            "ali@example.com",
            ["warehouseManager"],
            [Guid.CreateVersion7()]
        );
        using HttpResponseMessage accepted = await CreateAsync("ali@example.com", ["warehouseManager"], [active]);

        withNone.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(withNone)).ShouldBe("BR-SYS-014");
        (await CodeOfAsync(withInactive)).ShouldBe("BR-SYS-014");
        (await CodeOfAsync(withUnknown)).ShouldBe("BR-SYS-014");
        accepted.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task Edit_GivesTheUsersOpenSessionTheNewRolesPermissionsAtOnce()
    {
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        using HttpClient ali = await SignedInAsync("ali@example.com", Password);
        using HttpResponseMessage before = await ali.GetAsync(new Uri("/api/v1/users", UriKind.Relative), Cancellation);

        using HttpResponseMessage edited = await EditAsync(
            user,
            "ali@example.com",
            ["bookingManager", "systemAdministrator"]
        );
        using HttpResponseMessage after = await ali.GetAsync(new Uri("/api/v1/users", UriKind.Relative), Cancellation);

        before.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        edited.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement body = await edited.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        int version = body.GetProperty("version").GetInt32();
        edited.Headers.ETag!.Tag.ShouldBe(string.Create(CultureInfo.InvariantCulture, $"\"{version}\""));
        body.GetProperty("roles").GetArrayLength().ShouldBe(2);
        after.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await RoleChangesOfAsync(user)).ShouldBe(1);
    }

    [Fact]
    public async Task Edit_OfTheEmailEndsTheUsersSessions()
    {
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        using HttpClient ali = await SignedInAsync("ali@example.com", Password);

        using HttpResponseMessage edited = await EditAsync(user, "ali.bal@example.com", ["bookingManager"]);
        using HttpResponseMessage me = await ali.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation);

        edited.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-011")]
    public async Task Edit_BasedOnAnOldVersion_IsRefused()
    {
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        int version = await VersionOfAsync(user);
        using HttpResponseMessage first = await EditAsync(user, "ali@example.com", ["technicalManager"], version);

        using HttpResponseMessage second = await EditAsync(user, "ali@example.com", ["bookingManager"], version);

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-009")]
    public async Task TheLastActiveSystemAdministrator_KeepsTheRole_AndCannotDeactivateThemselves()
    {
        UserId admin = await IdOfAsync(AdminEmail);

        using HttpResponseMessage edited = await EditAsync(admin, AdminEmail, ["bookingManager"]);
        using HttpResponseMessage deactivated = await PostAsync(admin, "deactivate");

        edited.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(edited)).ShouldBe("BR-SYS-009");
        deactivated.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(deactivated)).ShouldBe("BR-SYS-009");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public async Task Deactivate_EndsTheUsersSessionsAndSignIn_UntilActivatedAgain()
    {
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        using HttpClient ali = await SignedInAsync("ali@example.com", Password);

        using HttpResponseMessage deactivated = await PostAsync(user, "deactivate");
        using HttpResponseMessage me = await ali.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation);
        using HttpResponseMessage refused = await LoginAsync("ali@example.com", Password);
        using HttpResponseMessage activated = await PostAsync(user, "activate");
        using HttpResponseMessage accepted = await LoginAsync("ali@example.com", Password);

        deactivated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await deactivated.Content.ReadFromJsonAsync<JsonElement>(Cancellation))
            .GetProperty("deactivatedAt")
            .ValueKind.ShouldBe(JsonValueKind.String);
        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        refused.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        activated.StatusCode.ShouldBe(HttpStatusCode.OK);
        accepted.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public async Task ResetPassword_GivesANewTemporaryPassword_LiftsTheLockAndEndsTheSessions()
    {
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.BookingManager);
        using HttpClient ali = await SignedInAsync("ali@example.com", Password);
        await LockDirectlyAsync(user);

        using HttpResponseMessage reset = await PostAsync(user, "reset-password");
        using HttpResponseMessage me = await ali.GetAsync(new Uri("/api/v1/me", UriKind.Relative), Cancellation);

        reset.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement body = await reset.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        body.GetProperty("user").GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
        body.GetProperty("user").GetProperty("lockedUntil").ValueKind.ShouldBe(JsonValueKind.Null);
        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using HttpClient again = await SignedInAsync(
            "ali@example.com",
            body.GetProperty("temporaryPassword").GetString()!
        );
        (await GetMeAsync(again)).GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task ADeactivatedWarehouse_LeavesItsManagers_WhoWaitForANewOne()
    {
        Guid warehouse = await AddWarehouseAsync("Merkez Depo");
        using HttpResponseMessage created = await CreateAsync("ali@example.com", ["warehouseManager"], [warehouse]);
        var user = Guid.Parse(
            (await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("id").GetString()!
        );

        await DeactivateWarehouseAsync(warehouse);
        await fixture
            .Services.GetRequiredService<OutboxProcessor>()
            .ProcessBatchAsync(InventoryModuleDefinition.ModuleName, Cancellation);

        using HttpResponseMessage listed = await Admin.GetAsync(
            new Uri("/api/v1/users?role=warehouseManager", UriKind.Relative),
            Cancellation
        );
        JsonElement row = (await listed.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("items")[0];
        row.GetProperty("id").GetString().ShouldBe(user.ToString());
        row.GetProperty("isActive").GetBoolean().ShouldBeTrue();
        row.GetProperty("needsWarehouse").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task ManagingUsers_NeedsTheMatchingPermissions()
    {
        UserId user = await AddUserAsync("Ali Bal", "ali@example.com", Role.GeneralManager);
        using HttpClient manager = await SignedInAsync("ali@example.com", Password);

        using HttpResponseMessage created = await manager.PostAsJsonAsync(
            new Uri("/api/v1/users", UriKind.Relative),
            new
            {
                fullName = "Cem Dal",
                email = "cem@example.com",
                roles = BookingManagerRole,
                warehouseIds = Array.Empty<Guid>(),
            },
            Cancellation
        );
        using var deactivate = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/users/{user.Value}/deactivate", UriKind.Relative)
        );
        deactivate.Headers.IfMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"1\""));
        using HttpResponseMessage deactivated = await manager.SendAsync(deactivate, Cancellation);

        created.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        deactivated.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> AddWarehouseAsync(string name, bool deactivated = false)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        InventoryDbContext context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var warehouse = Warehouse.Create(name, "İstanbul", "Depo Sk. 4");
        if (deactivated)
        {
            warehouse.Deactivate(Guid.CreateVersion7(), fixture.Time.GetUtcNow());
        }

        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync(Cancellation);
        return warehouse.Id.Value;
    }

    // Through the command, so the deactivation reaches the outbox as it does in the application.
    private async Task DeactivateWarehouseAsync(Guid warehouse)
    {
        await AddWarehouseAsync("Yedek Depo");
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<DeactivateWarehouseCommand, bool>>()
            .HandleAsync(new DeactivateWarehouseCommand(WarehouseId.From(warehouse)), Cancellation);
    }

    private Task<HttpResponseMessage> CreateAsync(string email, string[] roles, Guid[] warehouseIds) =>
        Admin.PostAsJsonAsync(
            new Uri("/api/v1/users", UriKind.Relative),
            new
            {
                fullName = "Ali Bal",
                email,
                roles,
                warehouseIds,
            },
            Cancellation
        );

    private async Task<HttpResponseMessage> EditAsync(UserId user, string email, string[] roles, int? version = null)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            new Uri($"/api/v1/users/{user.Value}", UriKind.Relative)
        )
        {
            Content = JsonContent.Create(
                new
                {
                    fullName = "Ali Bal",
                    email,
                    roles,
                    warehouseIds = Array.Empty<Guid>(),
                }
            ),
        };
        int expected = version ?? await VersionOfAsync(user);
        request.Headers.IfMatch.Add(
            new System.Net.Http.Headers.EntityTagHeaderValue(
                string.Create(CultureInfo.InvariantCulture, $"\"{expected}\"")
            )
        );
        return await Admin.SendAsync(request, Cancellation);
    }

    private async Task<HttpResponseMessage> PostAsync(UserId user, string action)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/users/{user.Value}/{action}", UriKind.Relative)
        );
        int expected = await VersionOfAsync(user);
        request.Headers.IfMatch.Add(
            new System.Net.Http.Headers.EntityTagHeaderValue(
                string.Create(CultureInfo.InvariantCulture, $"\"{expected}\"")
            )
        );
        return await Admin.SendAsync(request, Cancellation);
    }

    private async Task<int> VersionOfAsync(UserId user)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.Where(found => found.Id == user)
            .Select(found => found.Version)
            .SingleAsync(Cancellation);
    }

    private async Task<UserId> IdOfAsync(string email)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.Where(found => found.Email == email)
            .Select(found => found.Id)
            .SingleAsync(Cancellation);
    }

    // The change history keeps the roles' old and new values (US-SYS-001, criterion 5).
    private async Task<int> RoleChangesOfAsync(UserId user)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        List<string> changes = await scope
            .ServiceProvider.GetRequiredService<AuditDbContext>()
            .Set<AuditEntry>()
            .Where(entry => entry.EntityId == user.Value && entry.Action == AuditAction.Updated)
            .Select(entry => entry.Changes)
            .ToListAsync(Cancellation);
        return changes.Count(change => change.Contains("roles", StringComparison.OrdinalIgnoreCase));
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

    // Test data only: a deactivated user whose email stays taken.
    private async Task DeactivateDirectlyAsync(UserId user)
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

    // Test data only: an account locked by wrong passwords.
    private async Task LockDirectlyAsync(UserId user)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Users.Where(found => found.Id == user)
            .ExecuteUpdateAsync(
                row => row.SetProperty(found => found.LockedUntil, fixture.Time.GetUtcNow().AddMinutes(10)),
                Cancellation
            );
    }

    private async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        using HttpClient client = IdentityFixture.CreateClient(_app!);
        return await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password },
            Cancellation
        );
    }

    private async Task<HttpClient> SignedInAsync(string email, string password)
    {
        HttpClient client = IdentityFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }

    private static async Task<JsonElement> GetMeAsync(HttpClient client)
    {
        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/me", UriKind.Relative),
            Cancellation
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("code").GetString();
}
