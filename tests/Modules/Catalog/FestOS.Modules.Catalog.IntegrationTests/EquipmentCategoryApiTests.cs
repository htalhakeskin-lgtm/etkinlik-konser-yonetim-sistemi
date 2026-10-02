using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;

namespace FestOS.Modules.Catalog.IntegrationTests;

/// <summary>The category tree over HTTP (US-EQP-001, catalog §6).</summary>
public sealed class EquipmentCategoryApiTests(CatalogFixture fixture) : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _technical;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Technical => _technical!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        _technical = await SignedInAsync("teknik@example.com");
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
    public async Task Create_BuildsTheTree_AndTheListGivesEachCategoryItsPath()
    {
        string sound = await IdOfAsync(await CreateAsync("Ses", parentId: null));
        string microphone = await IdOfAsync(await CreateAsync("Mikrofon", sound));
        (await CreateAsync("Dinamik vokal", microphone)).Dispose();

        JsonElement list = await GetJsonAsync("/api/v1/equipment-categories");

        JsonElement vocal = list.EnumerateArray()
            .Single(item =>
                string.Equals(item.GetProperty("name").GetString(), "Dinamik vokal", StringComparison.Ordinal)
            );
        vocal
            .GetProperty("path")
            .EnumerateArray()
            .Select(step => step.GetString())
            .ShouldBe(["Ses", "Mikrofon", "Dinamik vokal"]);
        vocal.GetProperty("parentId").GetString().ShouldBe(microphone);
    }

    [Fact]
    [Trait("Rule", "BR-EQP-011")]
    public async Task Create_RefusesANameTakenUnderTheSameParent_ButNotUnderAnother()
    {
        string sound = await IdOfAsync(await CreateAsync("Ses", parentId: null));
        string light = await IdOfAsync(await CreateAsync("Işık", parentId: null));
        (await CreateAsync("Kablo", sound)).Dispose();

        using HttpResponseMessage sameParent = await CreateAsync("KABLO", sound);
        using HttpResponseMessage otherParent = await CreateAsync("Kablo", light);
        using HttpResponseMessage topLevel = await CreateAsync("ses", parentId: null);

        sameParent.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(sameParent)).ShouldBe("BR-EQP-011");
        otherParent.StatusCode.ShouldBe(HttpStatusCode.Created);
        topLevel.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(topLevel)).ShouldBe("BR-EQP-011");
    }

    [Fact]
    [Trait("Rule", "BR-EQP-002")]
    public async Task Edit_RefusesToMoveACategoryUnderItsOwnChild()
    {
        string sound = await IdOfAsync(await CreateAsync("Ses", parentId: null));
        string microphone = await IdOfAsync(await CreateAsync("Mikrofon", sound));

        using HttpResponseMessage refused = await EditAsync(sound, "Ses", microphone, version: 1);
        using HttpResponseMessage moved = await EditAsync(microphone, "Mikrofon", parentId: null, version: 1);

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-EQP-002");
        moved.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await moved.Content.ReadFromJsonAsync<JsonElement>(Cancellation))
            .GetProperty("parentId")
            .ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    [Trait("Rule", "BR-EQP-002")]
    public async Task Deactivate_KeepsACategoryWithActiveChildren_AndActivateNeedsAnActiveParent()
    {
        string sound = await IdOfAsync(await CreateAsync("Ses", parentId: null));
        string microphone = await IdOfAsync(await CreateAsync("Mikrofon", sound));

        using HttpResponseMessage refused = await PostAsync(sound, "deactivate", version: 1);
        using HttpResponseMessage child = await PostAsync(microphone, "deactivate", version: 1);
        using HttpResponseMessage parent = await PostAsync(sound, "deactivate", version: 1);
        using HttpResponseMessage underInactive = await PostAsync(microphone, "activate", version: 2);
        using HttpResponseMessage createUnderInactive = await CreateAsync("Kablosuz", sound);

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await refused.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        problem.GetProperty("code").GetString().ShouldBe("BR-EQP-002");
        problem.GetProperty("params").GetProperty("activeCategories").GetInt32().ShouldBe(1);
        child.StatusCode.ShouldBe(HttpStatusCode.OK);
        parent.StatusCode.ShouldBe(HttpStatusCode.OK);
        underInactive.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        createUnderInactive.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await GetJsonAsync("/api/v1/equipment-categories?status=inactive")).GetArrayLength().ShouldBe(2);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task TheBookingManager_SeesTheTree_ButOnlyTheTechnicalManagerChangesIt()
    {
        (await CreateAsync("Ses", parentId: null)).Dispose();
        await fixture.AddUserAsync("booking@example.com", Role.BookingManager);
        using HttpClient booking = await SignedInAsync("booking@example.com");

        using HttpResponseMessage listed = await booking.GetAsync(
            new Uri("/api/v1/equipment-categories", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage created = await booking.PostAsJsonAsync(
            new Uri("/api/v1/equipment-categories", UriKind.Relative),
            new { name = "Işık" },
            Cancellation
        );

        listed.StatusCode.ShouldBe(HttpStatusCode.OK);
        created.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private Task<HttpResponseMessage> CreateAsync(string name, string? parentId) =>
        Technical.PostAsJsonAsync(
            new Uri("/api/v1/equipment-categories", UriKind.Relative),
            new { name, parentId },
            Cancellation
        );

    private async Task<HttpResponseMessage> EditAsync(string id, string name, string? parentId, int version)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            new Uri($"/api/v1/equipment-categories/{id}", UriKind.Relative)
        )
        {
            Content = JsonContent.Create(new { name, parentId }),
        };
        request.Headers.IfMatch.Add(new EntityTagHeaderValue(Tag(version)));
        return await Technical.SendAsync(request, Cancellation);
    }

    private async Task<HttpResponseMessage> PostAsync(string id, string action, int version)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/equipment-categories/{id}/{action}", UriKind.Relative)
        );
        request.Headers.IfMatch.Add(new EntityTagHeaderValue(Tag(version)));
        return await Technical.SendAsync(request, Cancellation);
    }

    private static string Tag(int version) => string.Create(CultureInfo.InvariantCulture, $"\"{version}\"");

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
