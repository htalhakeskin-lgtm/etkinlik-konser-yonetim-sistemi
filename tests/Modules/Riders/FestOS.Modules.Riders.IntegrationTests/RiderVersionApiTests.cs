using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Riders.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Riders.IntegrationTests;

/// <summary>A rider's versions over HTTP (US-RDR-001, US-RDR-002, riders §6).</summary>
public sealed class RiderVersionApiTests(RidersFixture fixture) : IAsyncLifetime
{
    private static readonly string[] ArtistRole = ["artist"];

    private WebApplication? _app;
    private HttpClient? _booking;
    private HttpClient? _technical;
    private string _rider = string.Empty;
    private string _production = string.Empty;
    private string _category = string.Empty;
    private string _model = string.Empty;
    private string _other = string.Empty;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Booking => _booking!;

    private HttpClient Technical => _technical!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        await fixture.AddUserAsync("booking@example.com", Role.BookingManager);
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        _booking = await SignedInAsync("booking@example.com");
        _technical = await SignedInAsync("teknik@example.com");

        string artist = await IdOfAsync(
            await Booking.PostAsJsonAsync(
                new Uri("/api/v1/parties", UriKind.Relative),
                new
                {
                    kind = "organization",
                    name = "Gece Yolcuları",
                    roles = ArtistRole,
                    contactPoints = Array.Empty<object>(),
                },
                Cancellation
            )
        );
        using HttpResponseMessage production = await Booking.PostAsJsonAsync(
            new Uri("/api/v1/productions", UriKind.Relative),
            new { artistPartyId = artist, name = "Gece Turnesi 2027" },
            Cancellation
        );
        JsonElement created = await production.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        _production = created.GetProperty("id").GetString()!;
        _rider = created.GetProperty("riderId").GetString()!;

        _category = await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-categories", UriKind.Relative),
                new { name = "Mikrofon" },
                Cancellation
            )
        );
        _model = await ModelAsync("Shure", "SM58");
        _other = await ModelAsync("Sennheiser", "e935");
    }

    public async ValueTask DisposeAsync()
    {
        _booking?.Dispose();
        _technical?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Rule", "BR-RDR-001")]
    public async Task CreateVersion_SavesTheLinesWithNames_TheSaversName_AndTheEvent()
    {
        using HttpResponseMessage saved = await SaveAsync(
            riderVersion: 1,
            new
            {
                note = "2027 turnesi",
                lines = new object[]
                {
                    new
                    {
                        modelId = _model,
                        quantity = 6,
                        flexibility = "flexible",
                        equivalentModelIds = new[] { _other },
                        note = "Vokal",
                    },
                    new { categoryId = _category, quantity = 2 },
                },
            }
        );

        saved.StatusCode.ShouldBe(HttpStatusCode.Created, await saved.Content.ReadAsStringAsync(Cancellation));
        JsonElement version = await saved.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        version.GetProperty("number").GetInt32().ShouldBe(1);
        version.GetProperty("createdByName").GetString().ShouldNotBeNullOrEmpty();
        JsonElement lines = version.GetProperty("lines");
        lines[0].GetProperty("name").GetString().ShouldBe("Shure SM58");
        lines[0].GetProperty("categoryPath")[0].GetString().ShouldBe("Mikrofon");
        lines[0].GetProperty("flexibility").GetString().ShouldBe("flexible");
        lines[0].GetProperty("equivalents")[0].GetProperty("name").GetString().ShouldBe("Sennheiser e935");
        lines[1].GetProperty("name").GetString().ShouldBe("Mikrofon");
        lines[1].GetProperty("flexibility").ValueKind.ShouldBe(JsonValueKind.Null);

        JsonElement list = await GetJsonAsync(Booking, $"/api/v1/riders/{_rider}/versions");
        list.GetProperty("version").GetInt32().ShouldBe(2);
        list.GetProperty("versions")[0].GetProperty("lineCount").GetInt32().ShouldBe(2);
        list.GetProperty("versions")[0].GetProperty("note").GetString().ShouldBe("2027 turnesi");
        (await GetJsonAsync(Booking, $"/api/v1/productions/{_production}"))
            .GetProperty("latestVersionNumber")
            .GetInt32()
            .ShouldBe(1);
        JsonElement row = (await GetJsonAsync(Booking, "/api/v1/productions")).GetProperty("items")[0];
        row.GetProperty("latestVersionNumber").GetInt32().ShouldBe(1);
        row.GetProperty("latestVersionAt").GetString().ShouldNotBeNullOrEmpty();

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        List<string> events = await scope
            .ServiceProvider.GetRequiredService<RidersDbContext>()
            .Set<OutboxMessage>()
            .Select(message => message.Payload)
            .ToListAsync(Cancellation);
        events.ShouldHaveSingleItem().ShouldContain(_rider);
    }

    [Fact]
    [Trait("Rule", "BR-RDR-003")]
    public async Task CreateVersion_NumbersTheNextVersion_KeepsLineKeys_AndRefusesAStaleRider()
    {
        JsonElement first = await SavedAsync(1, new { lines = new[] { ModelLine(_model) } });
        string key = first.GetProperty("lines")[0].GetProperty("lineKey").GetString()!;

        JsonElement second = await SavedAsync(
            2,
            new
            {
                lines = new[]
                {
                    new
                    {
                        lineKey = key,
                        modelId = _model,
                        quantity = 8,
                        flexibility = "required",
                    },
                },
            }
        );
        using HttpResponseMessage stale = await SaveAsync(2, new { lines = new[] { ModelLine(_model) } });

        second.GetProperty("number").GetInt32().ShouldBe(2);
        second.GetProperty("lines")[0].GetProperty("lineKey").GetString().ShouldBe(key);
        stale.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        JsonElement firstAgain = await GetJsonAsync(
            Booking,
            $"/api/v1/rider-versions/{first.GetProperty("id").GetString()}"
        );
        firstAgain.GetProperty("lines")[0].GetProperty("quantity").GetInt32().ShouldBe(4);
    }

    [Fact]
    [Trait("Rule", "BR-RDR-001")]
    public async Task CreateVersion_RefusesAnInactiveModel()
    {
        using var deactivate = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/equipment-models/{_model}/deactivate", UriKind.Relative)
        );
        deactivate.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));
        (await Technical.SendAsync(deactivate, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using HttpResponseMessage refused = await SaveAsync(1, new { lines = new[] { ModelLine(_model) } });

        refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await refused.Content.ReadAsStringAsync(Cancellation)).ShouldContain("/lines/0/modelId");
    }

    [Fact]
    [Trait("Rule", "BR-RDR-002")]
    public async Task CreateVersion_RefusesEquivalentsOnARequiredLine()
    {
        using HttpResponseMessage refused = await SaveAsync(
            1,
            new
            {
                lines = new[]
                {
                    new
                    {
                        modelId = _model,
                        quantity = 1,
                        flexibility = "required",
                        equivalentModelIds = new[] { _other },
                    },
                },
            }
        );

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await refused.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        problem.GetProperty("code").GetString().ShouldBe("BR-RDR-002");
        problem.GetProperty("params").GetProperty("reason").GetString().ShouldBe("notFlexible");
    }

    [Fact]
    [Trait("Rule", "BR-RDR-008")]
    public async Task OnlyTheTechnicalManager_SavesAVersion_TheBookingManagerReads()
    {
        using HttpResponseMessage refused = await SaveAsync(1, new { lines = new[] { ModelLine(_model) } }, Booking);
        using HttpResponseMessage listed = await Booking.GetAsync(
            new Uri($"/api/v1/riders/{_rider}/versions", UriKind.Relative),
            Cancellation
        );

        refused.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        listed.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static object ModelLine(string modelId) =>
        new
        {
            modelId,
            quantity = 4,
            flexibility = "required",
        };

    private async Task<string> ModelAsync(string brand, string name) =>
        await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-models", UriKind.Relative),
                new
                {
                    brand,
                    name,
                    categoryId = _category,
                    trackingType = "serialized",
                },
                Cancellation
            )
        );

    private async Task<JsonElement> SavedAsync(int riderVersion, object body)
    {
        using HttpResponseMessage saved = await SaveAsync(riderVersion, body);
        saved.StatusCode.ShouldBe(HttpStatusCode.Created, await saved.Content.ReadAsStringAsync(Cancellation));
        return await saved.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private async Task<HttpResponseMessage> SaveAsync(int riderVersion, object body, HttpClient? client = null)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/riders/{_rider}/versions", UriKind.Relative)
        )
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.IfMatch.Add(
            new EntityTagHeaderValue(string.Create(CultureInfo.InvariantCulture, $"\"{riderVersion}\""))
        );
        return await (client ?? Technical).SendAsync(request, Cancellation);
    }

    private static async Task<JsonElement> GetJsonAsync(HttpClient client, string address)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
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

    private async Task<HttpClient> SignedInAsync(string email)
    {
        HttpClient client = RidersFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = RidersFixture.Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
