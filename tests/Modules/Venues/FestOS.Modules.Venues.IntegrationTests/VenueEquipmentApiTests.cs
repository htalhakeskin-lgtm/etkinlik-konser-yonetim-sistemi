using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Venues.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Venues.IntegrationTests;

/// <summary>A venue's equipment over HTTP (US-VEN-002, venues §6).</summary>
public sealed class VenueEquipmentApiTests(VenuesFixture fixture) : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _technical;
    private string _venue = string.Empty;
    private string _model = string.Empty;
    private int _version = 1;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Technical => _technical!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        await fixture.AddUserAsync("booking@example.com", Role.BookingManager);
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        using HttpClient booking = await SignedInAsync("booking@example.com");
        _venue = await IdOfAsync(
            await booking.PostAsJsonAsync(
                new Uri("/api/v1/venues", UriKind.Relative),
                new
                {
                    name = "Açıkhava",
                    city = "İstanbul",
                    address = "Harbiye",
                    capacity = 4000,
                },
                Cancellation
            )
        );
        _technical = await SignedInAsync("teknik@example.com");
        string category = await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-categories", UriKind.Relative),
                new { name = "Ses masası" },
                Cancellation
            )
        );
        _model = await IdOfAsync(
            await Technical.PostAsJsonAsync(
                new Uri("/api/v1/equipment-models", UriKind.Relative),
                new
                {
                    brand = "Yamaha",
                    name = "CL5",
                    categoryId = category,
                    trackingType = "serialized",
                },
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
    [Trait("Rule", "BR-VEN-001")]
    public async Task Lines_ShowTheirModelNames_AndTheUsableQuantityLeavesFreeDescriptionsOut()
    {
        JsonElement withConsole = await SendAsync(HttpMethod.Post, "equipment", new { modelId = _model, quantity = 2 });
        string line = withConsole.GetProperty("lines")[0].GetProperty("id").GetString()!;
        await SendAsync(HttpMethod.Post, "equipment", new { description = "Kuyruklu piyano", quantity = 1 });
        await SendAsync(
            HttpMethod.Post,
            $"equipment/{line}/unavailabilities",
            new
            {
                periodStart = "2027-03-10",
                periodEnd = "2027-03-12",
                quantity = 1,
                reason = "Başka etkinliğe verildi",
            }
        );

        JsonElement list = await GetJsonAsync($"/api/v1/venues/{_venue}/equipment");
        JsonElement usable = await GetJsonAsync(
            $"/api/v1/venues/{_venue}/equipment/usable?from=2027-03-11&to=2027-03-12"
        );

        JsonElement console = list.GetProperty("lines")[0];
        console.GetProperty("name").GetString().ShouldBe("Yamaha CL5");
        console.GetProperty("isCounted").GetBoolean().ShouldBeTrue();
        console
            .GetProperty("unavailabilities")[0]
            .GetProperty("reason")
            .GetString()
            .ShouldBe("Başka etkinliğe verildi");
        list.GetProperty("lines")[1].GetProperty("isCounted").GetBoolean().ShouldBeFalse();
        usable[0].GetProperty("usableQuantity").GetInt32().ShouldBe(1);
        usable[1].GetProperty("usableQuantity").GetInt32().ShouldBe(0);
    }

    [Fact]
    [Trait("Rule", "BR-VEN-002")]
    public async Task OverlappingPeriods_AreRefused_AndAChangeIsAnnouncedWithItsDays()
    {
        JsonElement withConsole = await SendAsync(HttpMethod.Post, "equipment", new { modelId = _model, quantity = 2 });
        string line = withConsole.GetProperty("lines")[0].GetProperty("id").GetString()!;
        await SendAsync(
            HttpMethod.Post,
            $"equipment/{line}/unavailabilities",
            new
            {
                periodStart = "2027-03-10",
                periodEnd = "2027-03-12",
                quantity = 1,
                reason = "Ödünç",
            }
        );

        using HttpResponseMessage refused = await RawSendAsync(
            HttpMethod.Post,
            $"equipment/{line}/unavailabilities",
            new
            {
                periodStart = "2027-03-11",
                periodEnd = "2027-03-13",
                quantity = 1,
                reason = "Çakışan",
            }
        );

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await refused.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        problem.GetProperty("code").GetString().ShouldBe("BR-VEN-002");
        problem.GetProperty("params").GetProperty("reason").GetString().ShouldBe("overlap");
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        List<string> events = await scope
            .ServiceProvider.GetRequiredService<VenuesDbContext>()
            .Set<OutboxMessage>()
            .OrderBy(message => message.Sequence)
            .Select(message => message.Payload)
            .ToListAsync(Cancellation);
        events.Count.ShouldBe(2);
        events[1].ShouldContain("2027-03-10");
        events[1].ShouldContain("2027-03-12");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task OnlyTheTechnicalManager_EntersEquipment()
    {
        using HttpClient booking = await SignedInAsync("booking@example.com");
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/venues/{_venue}/equipment", UriKind.Relative)
        )
        {
            Content = JsonContent.Create(new { description = "Piyano", quantity = 1 }),
        };
        request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));

        using HttpResponseMessage response = await booking.SendAsync(request, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task<JsonElement> SendAsync(HttpMethod method, string path, object body)
    {
        using HttpResponseMessage response = await RawSendAsync(method, path, body);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        _version = int.Parse(response.Headers.ETag!.Tag.Trim('"'), System.Globalization.CultureInfo.InvariantCulture);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private async Task<HttpResponseMessage> RawSendAsync(HttpMethod method, string path, object body)
    {
        using var request = new HttpRequestMessage(method, new Uri($"/api/v1/venues/{_venue}/{path}", UriKind.Relative))
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.IfMatch.Add(
            new EntityTagHeaderValue(
                string.Create(System.Globalization.CultureInfo.InvariantCulture, $"\"{_version}\"")
            )
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

    private async Task<HttpClient> SignedInAsync(string email)
    {
        HttpClient client = VenuesFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = VenuesFixture.Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
