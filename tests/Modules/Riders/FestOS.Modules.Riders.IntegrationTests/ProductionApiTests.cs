using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;

namespace FestOS.Modules.Riders.IntegrationTests;

/// <summary>The productions over HTTP (US-ART-001, riders §6).</summary>
public sealed class ProductionApiTests(RidersFixture fixture) : IAsyncLifetime
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
    public async Task Create_TakesAnArtist_AndOpensAnEmptyRider()
    {
        string artist = await PartyAsync("Gece Yolcuları", "artist");

        using HttpResponseMessage created = await CreateAsync(artist, "Gece Turnesi 2027");
        JsonElement list = await GetJsonAsync($"/api/v1/productions?artistId={artist}");

        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
        JsonElement production = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        production.GetProperty("artistName").GetString().ShouldBe("Gece Yolcuları");
        production.GetProperty("isArtistActive").GetBoolean().ShouldBeTrue();
        production.GetProperty("riderId").GetString().ShouldNotBeNullOrEmpty();
        production.GetProperty("riderVersion").GetInt32().ShouldBe(1);
        production.GetProperty("latestVersionNumber").GetInt32().ShouldBe(0);
        list.GetProperty("totalCount").GetInt32().ShouldBe(1);
        list.GetProperty("items")[0].GetProperty("artistName").GetString().ShouldBe("Gece Yolcuları");
    }

    [Fact]
    [Trait("Rule", "BR-PTY-004")]
    public async Task Create_RefusesAPartyWithoutTheArtistRole()
    {
        string agency = await PartyAsync("Sahne Ajans", "agency");

        using HttpResponseMessage refused = await CreateAsync(agency, "Gece Turnesi 2027");

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonElement problem = await refused.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        problem.GetProperty("code").GetString().ShouldBe("BR-PTY-004");
        problem.GetProperty("params").GetProperty("role").GetString().ShouldBe("artist");
    }

    [Fact]
    [Trait("Rule", "BR-RDR-009")]
    public async Task Create_RefusesANameTakenForTheSameArtist_ButNotForAnother()
    {
        string first = await PartyAsync("Gece Yolcuları", "artist");
        string second = await PartyAsync("Mavi Sesler", "artist");
        (await CreateAsync(first, "Akustik Gece")).Dispose();

        using HttpResponseMessage sameArtist = await CreateAsync(first, "AKUSTİK GECE");
        using HttpResponseMessage otherArtist = await CreateAsync(second, "Akustik Gece");

        sameArtist.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(sameArtist)).ShouldBe("BR-RDR-009");
        otherArtist.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Edit_ChangesTheNameOnTheVersion_AndKeepsTheArtist()
    {
        string artist = await PartyAsync("Gece Yolcuları", "artist");
        string production = await IdOfAsync(await CreateAsync(artist, "Akustik"));

        using HttpResponseMessage edited = await SendAsync(
            HttpMethod.Put,
            $"/api/v1/productions/{production}",
            version: 1,
            JsonContent.Create(new { name = "Akustik Gece", description = "Yaylılarla" })
        );
        using HttpResponseMessage stale = await SendAsync(
            HttpMethod.Put,
            $"/api/v1/productions/{production}",
            version: 1,
            JsonContent.Create(new { name = "Başka" })
        );

        edited.StatusCode.ShouldBe(HttpStatusCode.OK, await edited.Content.ReadAsStringAsync(Cancellation));
        JsonElement saved = await edited.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        saved.GetProperty("name").GetString().ShouldBe("Akustik Gece");
        saved.GetProperty("artistPartyId").GetString().ShouldBe(artist);
        stale.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public async Task Deactivate_HidesTheProductionFromTheDefaultList_AndTheTechnicalManagerOnlyReads()
    {
        string artist = await PartyAsync("Gece Yolcuları", "artist");
        string production = await IdOfAsync(await CreateAsync(artist, "Akustik"));
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        using HttpClient technical = await SignedInAsync("teknik@example.com");

        using HttpResponseMessage deactivated = await SendAsync(
            HttpMethod.Post,
            $"/api/v1/productions/{production}/deactivate",
            version: 1,
            content: null
        );
        using HttpResponseMessage listed = await technical.GetAsync(
            new Uri("/api/v1/productions?status=all", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage created = await technical.PostAsJsonAsync(
            new Uri("/api/v1/productions", UriKind.Relative),
            new { artistPartyId = artist, name = "Yeni" },
            Cancellation
        );

        deactivated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetJsonAsync("/api/v1/productions")).GetProperty("totalCount").GetInt32().ShouldBe(0);
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

    private Task<HttpResponseMessage> CreateAsync(string artistId, string name) =>
        Booking.PostAsJsonAsync(
            new Uri("/api/v1/productions", UriKind.Relative),
            new { artistPartyId = artistId, name },
            Cancellation
        );

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string address,
        int version,
        HttpContent? content
    )
    {
        using var request = new HttpRequestMessage(method, new Uri(address, UriKind.Relative)) { Content = content };
        request.Headers.IfMatch.Add(
            new EntityTagHeaderValue(string.Create(CultureInfo.InvariantCulture, $"\"{version}\""))
        );
        return await Booking.SendAsync(request, Cancellation);
    }

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
