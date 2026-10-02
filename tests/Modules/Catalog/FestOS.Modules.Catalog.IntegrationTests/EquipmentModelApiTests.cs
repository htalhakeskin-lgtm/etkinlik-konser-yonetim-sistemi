using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Catalog.IntegrationTests;

/// <summary>The models over HTTP (US-EQP-002, catalog §6).</summary>
public sealed class EquipmentModelApiTests(CatalogFixture fixture) : IAsyncLifetime
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
    public async Task Create_AnswersWithTheModelAndItsCategoryPath_AndTheListFiltersByTheSubtree()
    {
        string sound = await CategoryAsync("Ses", parentId: null);
        string microphones = await CategoryAsync("Mikrofon", sound);
        string light = await CategoryAsync("Işık", parentId: null);

        using HttpResponseMessage created = await CreateAsync("Shure", "SM58", microphones, weight: "0.298");
        (await CreateAsync("Robe", "Spiider", light)).Dispose();
        JsonElement inSound = await GetJsonAsync($"/api/v1/equipment-models?categoryId={sound}");
        JsonElement bySearch = await GetJsonAsync("/api/v1/equipment-models?q=spiider");
        JsonElement categories = await GetJsonAsync("/api/v1/equipment-categories");

        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
        JsonElement body = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        body.GetProperty("categoryPath")
            .EnumerateArray()
            .Select(step => step.GetString())
            .ShouldBe(["Ses", "Mikrofon"]);
        body.GetProperty("weightKilograms").GetString().ShouldBe("0.298", "decimals travel as text (api A-11)");
        body.GetProperty("trackingType").GetString().ShouldBe("serialized");
        inSound
            .GetProperty("items")
            .EnumerateArray()
            .ShouldHaveSingleItem()
            .GetProperty("name")
            .GetString()
            .ShouldBe("SM58");
        bySearch.GetProperty("items")[0].GetProperty("brand").GetString().ShouldBe("Robe");
        categories
            .EnumerateArray()
            .Single(item => string.Equals(item.GetProperty("id").GetString(), microphones, StringComparison.Ordinal))
            .GetProperty("activeModelCount")
            .GetInt32()
            .ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-EQP-012")]
    public async Task Create_RefusesABrandAndNameTaken_WhateverTheCase()
    {
        string microphones = await CategoryAsync("Mikrofon", parentId: null);
        (await CreateAsync("Shure", "SM58", microphones)).Dispose();

        using HttpResponseMessage refused = await CreateAsync("SHURE", "sm58", microphones);

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-EQP-012");
    }

    [Fact]
    [Trait("Rule", "BR-EQP-002")]
    public async Task ACategory_WithAnActiveModel_StaysActive_AndAnInactiveOneTakesNoModel()
    {
        string microphones = await CategoryAsync("Mikrofon", parentId: null);
        string old = await CategoryAsync("Eski", parentId: null);
        string model = await IdOfAsync(await CreateAsync("Shure", "SM58", microphones));
        (await PostAsync("equipment-categories", old, "deactivate", version: 1)).Dispose();

        using HttpResponseMessage keeps = await PostAsync(
            "equipment-categories",
            microphones,
            "deactivate",
            version: 1
        );
        using HttpResponseMessage intoInactive = await CreateAsync("Shure", "SM57", old);
        using HttpResponseMessage deactivated = await PostAsync("equipment-models", model, "deactivate", version: 1);
        using HttpResponseMessage nowEmpty = await PostAsync(
            "equipment-categories",
            microphones,
            "deactivate",
            version: 1
        );

        keeps.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await keeps.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        problem.GetProperty("code").GetString().ShouldBe("BR-EQP-002");
        problem.GetProperty("params").GetProperty("activeModels").GetInt32().ShouldBe(1);
        intoInactive.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(intoInactive)).ShouldBe("BR-EQP-002");
        deactivated.StatusCode.ShouldBe(HttpStatusCode.OK);
        nowEmpty.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Directory_TellsOtherModulesTheNamesPathsAndStatus()
    {
        string sound = await CategoryAsync("Ses", parentId: null);
        string microphones = await CategoryAsync("Mikrofon", sound);
        string model = await IdOfAsync(await CreateAsync("Shure", "SM58", microphones));

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        ICatalogDirectory directory = scope.ServiceProvider.GetRequiredService<ICatalogDirectory>();
        IReadOnlyDictionary<Guid, ModelSummary> models = await directory.FindModelsAsync(
            [Guid.Parse(model), Guid.CreateVersion7()],
            Cancellation
        );
        IReadOnlyDictionary<Guid, CategorySummary> categories = await directory.FindCategoriesAsync(
            [Guid.Parse(microphones)],
            Cancellation
        );

        ModelSummary found = models.ShouldHaveSingleItem().Value;
        found.DisplayName.ShouldBe("Shure SM58");
        found.CategoryPath.ShouldBe(["Ses", "Mikrofon"]);
        found.TrackingType.ShouldBe("serialized");
        found.IsActive.ShouldBeTrue();
        categories[Guid.Parse(microphones)].Path.ShouldBe(["Ses", "Mikrofon"]);
    }

    private async Task<string> CategoryAsync(string name, string? parentId) =>
        await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-categories", UriKind.Relative),
                new { name, parentId },
                Cancellation
            )
        );

    private Task<HttpResponseMessage> CreateAsync(
        string brand,
        string name,
        string categoryId,
        string? weight = null
    ) =>
        Technical.PostAsJsonAsync(
            new Uri("/api/v1/equipment-models", UriKind.Relative),
            new
            {
                brand,
                name,
                categoryId,
                trackingType = "serialized",
                weightKilograms = weight,
            },
            Cancellation
        );

    private async Task<HttpResponseMessage> PostAsync(string resource, string id, string action, int version)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/{resource}/{id}/{action}", UriKind.Relative)
        );
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
