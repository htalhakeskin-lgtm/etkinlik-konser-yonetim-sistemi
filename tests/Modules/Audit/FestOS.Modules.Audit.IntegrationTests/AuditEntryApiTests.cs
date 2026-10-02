using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Audit.IntegrationTests;

/// <summary>The change history over HTTP (US-SYS-004, audit §3).</summary>
public sealed class AuditEntryApiTests(AuditFixture fixture) : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _admin;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Admin => _admin!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        await fixture.AddUserAsync("zeynep@example.com", Role.SystemAdministrator);
        _admin = await SignedInAsync("zeynep@example.com");
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
    public async Task History_ListsTheNewestFirst_WithWhoAndWhatChanged_OneSliceAtATime()
    {
        string first = await CreateWarehouseAsync("Merkez Depo");
        fixture.Time.Advance(TimeSpan.FromMinutes(1));
        string second = await CreateWarehouseAsync("Kuzey Depo");

        JsonElement firstSlice = await GetJsonAsync("/api/v1/audit-entries?module=inventory&limit=1");
        string cursor = firstSlice.GetProperty("nextCursor").GetString()!;
        JsonElement secondSlice = await GetJsonAsync(
            $"/api/v1/audit-entries?module=inventory&limit=1&after={Uri.EscapeDataString(cursor)}"
        );

        JsonElement newest = firstSlice.GetProperty("items")[0];
        newest.GetProperty("entityId").GetString().ShouldBe(second);
        newest.GetProperty("entityType").GetString().ShouldBe("Warehouse");
        newest.GetProperty("action").GetString().ShouldBe("created");
        newest.GetProperty("actorName").GetString().ShouldBe("Ayşe Kaya");
        newest.GetProperty("changes").GetProperty("name").GetProperty("new").GetString().ShouldBe("Kuzey Depo");
        secondSlice.GetProperty("items")[0].GetProperty("entityId").GetString().ShouldBe(first);
        secondSlice.GetProperty("nextCursor").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task History_OfOneRecord_IsTheSameListFilteredByItsRoot()
    {
        string warehouse = await CreateWarehouseAsync("Merkez Depo");
        await CreateWarehouseAsync("Kuzey Depo");

        JsonElement history = await GetJsonAsync($"/api/v1/audit-entries?rootType=Warehouse&rootId={warehouse}");

        history.GetProperty("items").GetArrayLength().ShouldBe(1);
        history.GetProperty("items")[0].GetProperty("entityId").GetString().ShouldBe(warehouse);
        history.GetProperty("items")[0].GetProperty("rootId").GetString().ShouldBe(warehouse);
    }

    [Fact]
    public async Task History_FiltersByIstanbulDays()
    {
        // Saved by the module's own command on two days; a browser session would not outlive the gap.
        await CreateWarehouseDirectlyAsync("Merkez Depo");
        fixture.Time.Advance(TimeSpan.FromDays(1));
        WarehouseId nextDay = await CreateWarehouseDirectlyAsync("Kuzey Depo");
        using HttpClient client = await SignedInAsync("zeynep@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/audit-entries?module=inventory&from=2027-01-05&to=2027-01-06", UriKind.Relative),
            Cancellation
        );

        JsonElement history = await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        history.GetProperty("items").GetArrayLength().ShouldBe(1);
        history.GetProperty("items")[0].GetProperty("entityId").GetString().ShouldBe(nextDay.Value.ToString());
    }

    [Fact]
    public async Task History_RefusesACursorItDidNotMake()
    {
        using HttpResponseMessage response = await Admin.GetAsync(
            new Uri("/api/v1/audit-entries?after=bm90LWEtY3Vyc29y", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [Trait("Rule", "BR-SYS-010")]
    [InlineData(Role.GeneralManager, HttpStatusCode.OK)]
    [InlineData(Role.BookingManager, HttpStatusCode.Forbidden)]
    [InlineData(Role.WarehouseManager, HttpStatusCode.Forbidden)]
    public async Task History_IsReadByTheAdministratorAndTheGeneralManagerOnly(Role role, HttpStatusCode status)
    {
        await fixture.AddUserAsync("someone@example.com", role);
        using HttpClient client = await SignedInAsync("someone@example.com");

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/v1/audit-entries", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(status);
    }

    private async Task<WarehouseId> CreateWarehouseDirectlyAsync(string name)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<CreateWarehouseCommand, WarehouseId>>()
            .HandleAsync(new CreateWarehouseCommand(name, "İstanbul", "Depo Sk. 4"), Cancellation);
    }

    private async Task<string> CreateWarehouseAsync(string name)
    {
        using HttpResponseMessage created = await Admin.PostAsJsonAsync(
            new Uri("/api/v1/warehouses", UriKind.Relative),
            new
            {
                name,
                city = "İstanbul",
                address = "Depo Sk. 4",
            },
            Cancellation
        );
        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
        return (await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("id").GetString()!;
    }

    private async Task<JsonElement> GetJsonAsync(string address)
    {
        using HttpResponseMessage response = await Admin.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private async Task<HttpClient> SignedInAsync(string email)
    {
        HttpClient client = AuditFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = AuditFixture.Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
