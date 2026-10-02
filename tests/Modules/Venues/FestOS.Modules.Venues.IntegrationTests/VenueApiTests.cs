using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;

namespace FestOS.Modules.Venues.IntegrationTests;

/// <summary>The venues over HTTP (US-VEN-001, venues §6).</summary>
public sealed class VenueApiTests(VenuesFixture fixture) : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _booking;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Booking => _booking!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
        await fixture.AddUserAsync("booking@example.com", Role.BookingManager);
        _booking = await SignedInAsync("booking@example.com");
    }

    public async ValueTask DisposeAsync()
    {
        _booking?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Rule", "BR-PTY-004")]
    public async Task Create_TakesAVenueOperator_AndAnswersWithItsName()
    {
        string operatorId = await PartyAsync("Açıkhava İşletme", "venueOperator");

        using HttpResponseMessage created = await CreateAsync("Açıkhava Tiyatrosu", "İstanbul", operatorId);
        JsonElement list = await GetJsonAsync("/api/v1/venues?city=istanbul");

        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
        JsonElement venue = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        venue.GetProperty("operatorName").GetString().ShouldBe("Açıkhava İşletme");
        venue.GetProperty("stageWidthMeters").GetString().ShouldBe("18.50");
        venue.GetProperty("curfew").GetString().ShouldBe("23:00:00");
        venue.GetProperty("timeZone").GetString().ShouldBe("Europe/Istanbul");
        list.GetProperty("items")[0].GetProperty("operatorName").GetString().ShouldBe("Açıkhava İşletme");
    }

    [Fact]
    [Trait("Rule", "BR-PTY-004")]
    public async Task Create_RefusesAnOperatorWithoutTheRole()
    {
        string supplier = await PartyAsync("Işık Ses", "supplier");

        using HttpResponseMessage refused = await CreateAsync("Açıkhava Tiyatrosu", "İstanbul", supplier);

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await refused.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        problem.GetProperty("code").GetString().ShouldBe("BR-PTY-004");
        problem.GetProperty("params").GetProperty("role").GetString().ShouldBe("venueOperator");
    }

    [Fact]
    [Trait("Rule", "BR-VEN-003")]
    public async Task Create_RefusesANameTakenInTheSameCity_ButNotInAnother()
    {
        (await CreateAsync("Jolly Joker", "İstanbul", operatorId: null)).Dispose();

        using HttpResponseMessage sameCity = await CreateAsync("JOLLY JOKER", "istanbul", operatorId: null);
        using HttpResponseMessage otherCity = await CreateAsync("Jolly Joker", "Ankara", operatorId: null);

        sameCity.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(sameCity)).ShouldBe("BR-VEN-003");
        otherCity.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ChecksTheFields_AndTheTimeZone()
    {
        using HttpResponseMessage response = await Booking.PostAsJsonAsync(
            new Uri("/api/v1/venues", UriKind.Relative),
            new
            {
                name = "Açıkhava",
                city = "İstanbul",
                address = "Harbiye",
                capacity = 0,
                timeZone = "Mars/Olympus",
            },
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string body = await response.Content.ReadAsStringAsync(Cancellation);
        body.ShouldContain("capacity");
        body.ShouldContain("timeZone");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public async Task Deactivate_HidesTheVenueFromTheDefaultList_AndTheTechnicalManagerOnlyReads()
    {
        string venue = await IdOfAsync(await CreateAsync("Açıkhava", "İstanbul", operatorId: null));
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        using HttpClient technical = await SignedInAsync("teknik@example.com");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/venues/{venue}/deactivate", UriKind.Relative)
        );
        request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));
        using HttpResponseMessage deactivated = await Booking.SendAsync(request, Cancellation);
        using HttpResponseMessage listed = await technical.GetAsync(
            new Uri("/api/v1/venues?status=all", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage created = await technical.PostAsJsonAsync(
            new Uri("/api/v1/venues", UriKind.Relative),
            new
            {
                name = "Yeni",
                city = "İzmir",
                address = "A",
                capacity = 10,
            },
            Cancellation
        );

        deactivated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetJsonAsync("/api/v1/venues")).GetProperty("totalCount").GetInt32().ShouldBe(0);
        listed.StatusCode.ShouldBe(HttpStatusCode.OK);
        created.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task<string> PartyAsync(string name, string role) =>
        await IdOfAsync(
            await Booking.PostAsJsonAsync(
                new Uri("/api/v1/parties", UriKind.Relative),
                new
                {
                    kind = "organization",
                    name,
                    roles = new[] { role },
                    contactPoints = Array.Empty<object>(),
                },
                Cancellation
            )
        );

    private Task<HttpResponseMessage> CreateAsync(string name, string city, string? operatorId) =>
        Booking.PostAsJsonAsync(
            new Uri("/api/v1/venues", UriKind.Relative),
            new
            {
                name,
                city,
                address = "Harbiye Mah. Cumhuriyet Cad. 1",
                capacity = 4000,
                operatorPartyId = operatorId,
                stageWidthMeters = "18.5",
                curfew = "23:00:00",
            },
            Cancellation
        );

    private async Task<JsonElement> GetJsonAsync(string address)
    {
        using HttpResponseMessage response = await Booking.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
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
