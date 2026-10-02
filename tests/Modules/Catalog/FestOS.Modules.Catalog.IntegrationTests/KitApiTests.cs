using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;

namespace FestOS.Modules.Catalog.IntegrationTests;

/// <summary>The kits over HTTP (US-EQP-005, catalog §6).</summary>
public sealed class KitApiTests(CatalogFixture fixture) : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _technical;
    private string _category = string.Empty;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Technical => _technical!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        _technical = await SignedInAsync("teknik@example.com");
        _category = await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-categories", UriKind.Relative),
                new { name = "Işık" },
                Cancellation
            )
        );
    }

    public async ValueTask DisposeAsync()
    {
        _technical?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Rule", "BR-EQP-003")]
    public async Task Kit_OpensDownToModels_AndSumsWeightAndPower_MarkingMissingValues()
    {
        string spot = await ModelAsync("Robe", "Spiider", weight: "17.500", power: 600);
        string cable = await ModelAsync("Klotz", "DMX 10 m", weight: "0.800", power: null);
        string inner = await IdOfAsync(await CreateAsync("Kablo seti", [Line(cable, 2)]));

        using HttpResponseMessage created = await CreateAsync(
            "Küçük sahne ışık paketi",
            [Line(spot, 4), SubKit(inner, 3)]
        );
        JsonElement list = await GetJsonAsync("/api/v1/kits");

        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
        JsonElement kit = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        kit.GetProperty("lines")[1].GetProperty("name").GetString().ShouldBe("Kablo seti");
        JsonElement contents = kit.GetProperty("contents");
        contents[0].GetProperty("name").GetString().ShouldBe("Robe Spiider");
        contents[0].GetProperty("quantity").GetInt32().ShouldBe(4);
        contents[1].GetProperty("quantity").GetInt32().ShouldBe(6);
        JsonElement totals = kit.GetProperty("totals");
        decimal.Parse(totals.GetProperty("weightKilograms").GetString()!, CultureInfo.InvariantCulture).ShouldBe(74.8m);
        totals.GetProperty("isWeightComplete").GetBoolean().ShouldBeTrue();
        totals.GetProperty("powerWatts").GetInt32().ShouldBe(2400);
        totals.GetProperty("isPowerComplete").GetBoolean().ShouldBeFalse("the cable has no power value");
        list.GetProperty("items")[1].GetProperty("lineCount").GetInt32().ShouldBe(2);
    }

    [Fact]
    [Trait("Rule", "BR-EQP-003")]
    public async Task Edit_RefusesAKitThatWouldHoldItselfThroughAnother()
    {
        string spot = await ModelAsync("Robe", "Spiider", weight: null, power: null);
        string inner = await IdOfAsync(await CreateAsync("İç paket", [Line(spot, 1)]));
        string outer = await IdOfAsync(await CreateAsync("Dış paket", [SubKit(inner, 1)]));

        using HttpResponseMessage refused = await EditAsync(
            inner,
            "İç paket",
            [Line(spot, 1), SubKit(outer, 1)],
            version: 1
        );

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-EQP-003");
    }

    [Fact]
    [Trait("Rule", "BR-EQP-013")]
    public async Task Create_RefusesANameTaken_AndAnInactiveModelOnANewLine()
    {
        string spot = await ModelAsync("Robe", "Spiider", weight: null, power: null);
        (await CreateAsync("Paket", [Line(spot, 1)])).Dispose();
        await DeactivateModelAsync(spot);

        using HttpResponseMessage taken = await CreateAsync("PAKET", []);
        using HttpResponseMessage inactive = await CreateAsync("Yeni paket", [Line(spot, 1)]);

        taken.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(taken)).ShouldBe("BR-EQP-013");
        inactive.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Model_ShowsTheKitsThatHoldIt()
    {
        string spot = await ModelAsync("Robe", "Spiider", weight: null, power: null);
        (await CreateAsync("Paket", [Line(spot, 2)])).Dispose();

        JsonElement model = await GetJsonAsync($"/api/v1/equipment-models/{spot}");

        model
            .GetProperty("kits")
            .EnumerateArray()
            .ShouldHaveSingleItem()
            .GetProperty("name")
            .GetString()
            .ShouldBe("Paket");
    }

    private static object Line(string modelId, int quantity) => new { modelId, quantity };

    private static object SubKit(string subKitId, int quantity) => new { subKitId, quantity };

    private async Task<string> ModelAsync(string brand, string name, string? weight, int? power) =>
        await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-models", UriKind.Relative),
                new
                {
                    brand,
                    name,
                    categoryId = _category,
                    trackingType = "serialized",
                    weightKilograms = weight,
                    powerWatts = power,
                },
                Cancellation
            )
        );

    private async Task DeactivateModelAsync(string id)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/equipment-models/{id}/deactivate", UriKind.Relative)
        );
        request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));
        using HttpResponseMessage response = await Technical.SendAsync(request, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private Task<HttpResponseMessage> CreateAsync(string name, object[] lines) =>
        Technical.PostAsJsonAsync(new Uri("/api/v1/kits", UriKind.Relative), new { name, lines }, Cancellation);

    private async Task<HttpResponseMessage> EditAsync(string id, string name, object[] lines, int version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"/api/v1/kits/{id}", UriKind.Relative))
        {
            Content = JsonContent.Create(new { name, lines }),
        };
        request.Headers.IfMatch.Add(
            new EntityTagHeaderValue(string.Create(CultureInfo.InvariantCulture, $"\"{version}\""))
        );
        return await Technical.SendAsync(request, Cancellation);
    }

    private async Task<JsonElement> GetJsonAsync(string address)
    {
        using HttpResponseMessage response = await Technical.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
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
        HttpClient client = CatalogFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = CatalogFixture.Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
