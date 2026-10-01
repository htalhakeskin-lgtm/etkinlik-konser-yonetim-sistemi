using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Inventory.IntegrationTests;

/// <summary>The warehouses over HTTP (US-SYS-005, inventory §5).</summary>
public sealed class WarehouseApiTests(InventoryFixture fixture) : IAsyncLifetime
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
    public async Task Create_AnswersWithTheWarehouseAndItsAddress_AndListsIt()
    {
        using HttpResponseMessage created = await CreateAsync("Merkez Depo", "İstanbul");
        JsonElement page = await GetJsonAsync("/api/v1/warehouses?sort=-name");

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        JsonElement body = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        created.Headers.Location!.ToString().ShouldBe($"/api/v1/warehouses/{body.GetProperty("id").GetString()}");
        body.GetProperty("version").GetInt32().ShouldBe(1);
        page.GetProperty("items")[0].GetProperty("name").GetString().ShouldBe("Merkez Depo");
        page.GetProperty("items")[0].GetProperty("isActive").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-016")]
    public async Task Create_RefusesANameAlreadyTaken_WhateverTheCaseAndMarks()
    {
        (await CreateAsync("Işıklar Depo", "İzmir")).Dispose();

        using HttpResponseMessage refused = await CreateAsync("ISIKLAR DEPO", "Ankara");

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-SYS-016");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-011")]
    public async Task Edit_ChangesTheWarehouseOnItsVersion_AndRefusesAnOldOne()
    {
        string id = await IdOfAsync(await CreateAsync("Merkez Depo", "İstanbul"));

        using HttpResponseMessage edited = await EditAsync(id, "Kuzey Depo", version: 1);
        using HttpResponseMessage stale = await EditAsync(id, "Güney Depo", version: 1);

        edited.StatusCode.ShouldBe(HttpStatusCode.OK);
        edited.Headers.ETag!.Tag.ShouldBe("\"2\"");
        (await edited.Content.ReadFromJsonAsync<JsonElement>(Cancellation))
            .GetProperty("name")
            .GetString()
            .ShouldBe("Kuzey Depo");
        stale.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-013")]
    public async Task Deactivate_KeepsTheLastActiveWarehouse_ButNotTheOthers()
    {
        string first = await IdOfAsync(await CreateAsync("Merkez Depo", "İstanbul"));
        string second = await IdOfAsync(await CreateAsync("Kuzey Depo", "Ankara"));

        using HttpResponseMessage deactivated = await PostAsync(first, "deactivate", version: 1);
        using HttpResponseMessage refused = await PostAsync(second, "deactivate", version: 1);
        using HttpResponseMessage activated = await PostAsync(first, "activate", version: 2);

        deactivated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await deactivated.Content.ReadFromJsonAsync<JsonElement>(Cancellation))
            .GetProperty("deactivatedAt")
            .ValueKind.ShouldBe(JsonValueKind.String);
        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-SYS-013");
        activated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetJsonAsync("/api/v1/warehouses")).GetProperty("totalCount").GetInt32().ShouldBe(2);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task EveryRole_SeesTheWarehouses_ButOnlyTheAdministratorManagesThem()
    {
        (await CreateAsync("Merkez Depo", "İstanbul")).Dispose();
        await fixture.AddUserAsync("booking@example.com", Role.BookingManager);
        using HttpClient booking = await SignedInAsync("booking@example.com");

        using HttpResponseMessage listed = await booking.GetAsync(
            new Uri("/api/v1/warehouses", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage created = await booking.PostAsJsonAsync(
            new Uri("/api/v1/warehouses", UriKind.Relative),
            new
            {
                name = "Kuzey Depo",
                city = "Ankara",
                address = "Adres",
            },
            Cancellation
        );

        listed.StatusCode.ShouldBe(HttpStatusCode.OK);
        created.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Directory_TellsOtherModulesWhichWarehousesAreActive()
    {
        string active = await IdOfAsync(await CreateAsync("Merkez Depo", "İstanbul"));
        string inactive = await IdOfAsync(await CreateAsync("Kuzey Depo", "Ankara"));
        (await PostAsync(inactive, "deactivate", version: 1)).Dispose();

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IReadOnlySet<Guid> found = await scope
            .ServiceProvider.GetRequiredService<IWarehouseDirectory>()
            .FindActiveAsync([Guid.Parse(active), Guid.Parse(inactive), Guid.CreateVersion7()], Cancellation);

        found.ShouldBe([Guid.Parse(active)]);
    }

    private Task<HttpResponseMessage> CreateAsync(string name, string city) =>
        Admin.PostAsJsonAsync(
            new Uri("/api/v1/warehouses", UriKind.Relative),
            new
            {
                name,
                city,
                address = "Depo Sk. 4",
            },
            Cancellation
        );

    private async Task<HttpResponseMessage> EditAsync(string id, string name, int version)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            new Uri($"/api/v1/warehouses/{id}", UriKind.Relative)
        )
        {
            Content = JsonContent.Create(
                new
                {
                    name,
                    city = "İstanbul",
                    address = "Depo Sk. 4",
                }
            ),
        };
        request.Headers.IfMatch.Add(new EntityTagHeaderValue(Tag(version)));
        return await Admin.SendAsync(request, Cancellation);
    }

    private async Task<HttpResponseMessage> PostAsync(string id, string action, int version)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/warehouses/{id}/{action}", UriKind.Relative)
        );
        request.Headers.IfMatch.Add(new EntityTagHeaderValue(Tag(version)));
        return await Admin.SendAsync(request, Cancellation);
    }

    private static string Tag(int version) => string.Create(CultureInfo.InvariantCulture, $"\"{version}\"");

    private async Task<JsonElement> GetJsonAsync(string address)
    {
        using HttpResponseMessage response = await Admin.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private static async Task<string> IdOfAsync(HttpResponseMessage created)
    {
        using (created)
        {
            created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
            return (await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("id").GetString()!;
        }
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("code").GetString();

    private async Task<HttpClient> SignedInAsync(string email)
    {
        HttpClient client = InventoryFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = InventoryFixture.Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
