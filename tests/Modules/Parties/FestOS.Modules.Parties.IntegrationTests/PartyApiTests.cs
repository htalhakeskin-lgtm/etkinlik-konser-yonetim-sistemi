using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;

namespace FestOS.Modules.Parties.IntegrationTests;

/// <summary>The parties over HTTP (US-PTY-001, US-PTY-002, parties §8).</summary>
public sealed class PartyApiTests(PartiesFixture fixture) : IAsyncLifetime
{
    private static readonly string[] ContactOnly = ["contact"];

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
    [Trait("Rule", "BR-PTY-002")]
    public async Task Create_AnswersWithTheParty_AndTheListFindsItByPhoneDigitsAndRole()
    {
        using HttpResponseMessage created = await CreateAsync(
            Person("Tarkan", roles: ["artist"], contactPoints: [Phone("+90 532 111 22 33"), Phone("0212 444 55 66")])
        );
        JsonElement byPhone = await GetJsonAsync("/api/v1/parties?q=5321112233");
        JsonElement suppliers = await GetJsonAsync("/api/v1/parties?role=supplier");

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        JsonElement body = await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        created.Headers.Location!.ToString().ShouldBe($"/api/v1/parties/{body.GetProperty("id").GetString()}");
        body.GetProperty("kind").GetString().ShouldBe("person");
        body.GetProperty("contactPoints")[0].GetProperty("isPrimary").GetBoolean().ShouldBeTrue();
        body.GetProperty("contactPoints")[1].GetProperty("isPrimary").GetBoolean().ShouldBeFalse();
        JsonElement row = byPhone.GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        row.GetProperty("name").GetString().ShouldBe("Tarkan");
        row.GetProperty("primaryPhone").GetString().ShouldBe("+90 532 111 22 33");
        row.GetProperty("roles")[0].GetString().ShouldBe("artist");
        suppliers.GetProperty("totalCount").GetInt32().ShouldBe(0);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-001")]
    public async Task Create_WithoutARole_AnswersWithTheRule()
    {
        using HttpResponseMessage refused = await CreateAsync(Organization("Işık Ses", roles: []));

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-PTY-001");
    }

    [Fact]
    [Trait("Rule", "BR-PTY-002")]
    public async Task Create_WithTwoPrimariesOfOneKind_AnswersWithTheRule()
    {
        using HttpResponseMessage refused = await CreateAsync(
            Organization(
                "Işık Ses",
                roles: ["supplier"],
                contactPoints: [Phone("0212 111 22 33", isPrimary: true), Phone("0212 111 22 34", isPrimary: true)]
            )
        );

        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(refused)).ShouldBe("BR-PTY-002");
    }

    [Fact]
    public async Task Create_ChecksTheFieldsOfTheKind_AndTheContactPointFormat()
    {
        using HttpResponseMessage noFirstName = await CreateAsync(
            new
            {
                kind = "person",
                name = "Ayşe",
                lastName = "Kaya",
                roles = ContactOnly,
                contactPoints = Array.Empty<object>(),
            }
        );
        using HttpResponseMessage badEmail = await CreateAsync(
            Organization("Işık Ses", roles: ["supplier"], contactPoints: [Email("not-an-address")])
        );

        noFirstName.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        badEmail.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-011")]
    public async Task Edit_KeepsTheContactPointsSentBack_AndRefusesAnOldVersion()
    {
        JsonElement party = await BodyOfAsync(
            await CreateAsync(Organization("Işık Ses", roles: ["supplier"], contactPoints: [Phone("0212 111 22 33")]))
        );
        string id = party.GetProperty("id").GetString()!;
        string phoneId = party.GetProperty("contactPoints")[0].GetProperty("id").GetString()!;

        using HttpResponseMessage edited = await EditAsync(
            id,
            Organization(
                "Işık Ses",
                legalName: "Işık Ses Sistemleri A.Ş.",
                roles: ["supplier", "customer"],
                contactPoints: [Phone("0212 111 22 35", id: phoneId), Email("info@isikses.example")]
            ),
            version: 1
        );
        using HttpResponseMessage stale = await EditAsync(id, Organization("Işık", roles: ["supplier"]), version: 1);

        edited.StatusCode.ShouldBe(HttpStatusCode.OK);
        edited.Headers.ETag!.Tag.ShouldBe("\"2\"");
        JsonElement body = await edited.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        body.GetProperty("legalName").GetString().ShouldBe("Işık Ses Sistemleri A.Ş.");
        body.GetProperty("contactPoints")[0].GetProperty("id").GetString().ShouldBe(phoneId);
        body.GetProperty("contactPoints")[0].GetProperty("value").GetString().ShouldBe("0212 111 22 35");
        body.GetProperty("contactPoints")[1].GetProperty("value").GetString().ShouldBe("info@isikses.example");
        stale.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public async Task Deactivate_HidesThePartyFromTheDefaultList_AndActivateBringsItBack()
    {
        string id = (await BodyOfAsync(await CreateAsync(Organization("Işık Ses", roles: ["supplier"]))))
            .GetProperty("id")
            .GetString()!;

        using HttpResponseMessage deactivated = await PostAsync(id, "deactivate", version: 1);
        int activeAfterDeactivation = (await GetJsonAsync("/api/v1/parties")).GetProperty("totalCount").GetInt32();
        int inactive = (await GetJsonAsync("/api/v1/parties?status=inactive")).GetProperty("totalCount").GetInt32();
        using HttpResponseMessage activated = await PostAsync(id, "activate", version: 2);

        deactivated.StatusCode.ShouldBe(HttpStatusCode.OK);
        activeAfterDeactivation.ShouldBe(0);
        inactive.ShouldBe(1);
        activated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetJsonAsync("/api/v1/parties")).GetProperty("totalCount").GetInt32().ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-002")]
    public async Task TheTechnicalManager_SeesParties_ButOnlyTheBookingManagerChangesThem()
    {
        (await CreateAsync(Organization("Işık Ses", roles: ["supplier"]))).Dispose();
        await fixture.AddUserAsync("teknik@example.com", Role.TechnicalManager);
        using HttpClient technical = await SignedInAsync("teknik@example.com");

        using HttpResponseMessage listed = await technical.GetAsync(
            new Uri("/api/v1/parties", UriKind.Relative),
            Cancellation
        );
        using HttpResponseMessage created = await technical.PostAsJsonAsync(
            new Uri("/api/v1/parties", UriKind.Relative),
            Organization("Kuzey Ses", roles: ["supplier"]),
            Cancellation
        );

        listed.StatusCode.ShouldBe(HttpStatusCode.OK);
        created.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static object Person(string name, string[] roles, object[]? contactPoints = null) =>
        new
        {
            kind = "person",
            name,
            firstName = "Hüseyin Tarkan",
            lastName = "Tevetoğlu",
            roles,
            contactPoints = contactPoints ?? [],
        };

    private static object Organization(
        string name,
        string[] roles,
        object[]? contactPoints = null,
        string? legalName = null
    ) =>
        new
        {
            kind = "organization",
            name,
            legalName,
            roles,
            contactPoints = contactPoints ?? [],
        };

    private static object Phone(string value, bool isPrimary = false, string? id = null) =>
        new
        {
            id,
            kind = "phone",
            value,
            isPrimary,
        };

    private static object Email(string value) =>
        new
        {
            kind = "email",
            value,
            isPrimary = false,
        };

    private Task<HttpResponseMessage> CreateAsync(object body) =>
        Booking.PostAsJsonAsync(new Uri("/api/v1/parties", UriKind.Relative), body, Cancellation);

    private async Task<HttpResponseMessage> EditAsync(string id, object body, int version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"/api/v1/parties/{id}", UriKind.Relative))
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.IfMatch.Add(new EntityTagHeaderValue(Tag(version)));
        return await Booking.SendAsync(request, Cancellation);
    }

    private async Task<HttpResponseMessage> PostAsync(string id, string action, int version)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri($"/api/v1/parties/{id}/{action}", UriKind.Relative)
        );
        request.Headers.IfMatch.Add(new EntityTagHeaderValue(Tag(version)));
        return await Booking.SendAsync(request, Cancellation);
    }

    private static string Tag(int version) => string.Create(CultureInfo.InvariantCulture, $"\"{version}\"");

    private async Task<JsonElement> GetJsonAsync(string address)
    {
        using HttpResponseMessage response = await Booking.GetAsync(new Uri(address, UriKind.Relative), Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    private static async Task<JsonElement> BodyOfAsync(HttpResponseMessage created)
    {
        using (created)
        {
            created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync(Cancellation));
            return await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
        }
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("code").GetString();

    private async Task<HttpClient> SignedInAsync(string email)
    {
        HttpClient client = PartiesFixture.CreateClient(_app!);
        using HttpResponseMessage login = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new { email, password = PartiesFixture.Password },
            Cancellation
        );
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
