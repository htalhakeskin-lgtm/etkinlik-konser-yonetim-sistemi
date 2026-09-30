using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.Http;

/// <summary>The Idempotency-Key header and the request fingerprint through the real pipeline (api §10).</summary>
public sealed class IdempotencyFilterTests : IAsyncLifetime
{
    private const string Key = "0192f0a0-0000-7000-8000-000000000001";

    private WebApplication? _app;
    private HttpClient? _client;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Client => _client!;

    public async ValueTask InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddHttpPlatform();
        _app = builder.Build();
        _app.UseHttpPlatform();

        // As MapModules does for every module endpoint.
        RouteGroupBuilder api = _app.MapGroup("/api/v1").RequiresIdempotencyKey();
        api.MapGet("/things", () => "listed");
        api.MapPost(
            "/things",
            (ThingRequest request, IdempotencyRequest idempotency) =>
                new SeenRequest(idempotency.Key, idempotency.Fingerprint, request.Name)
        );
        api.MapPut("/things/1", () => TypedResults.NoContent()).RequiresVersion();

        await _app.StartAsync(Cancellation);
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task Get_NeedsNoKey()
    {
        using HttpResponseMessage response = await Client.GetAsync(
            new Uri("/api/v1/things", UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_WithoutAKey_IsRejected()
    {
        using HttpResponseMessage response = await PostAsync("/api/v1/things", """{ "name": "Truss" }""", key: null);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        problem.RootElement.GetProperty("code").GetString().ShouldBe("idempotencyKeyMissing");
        problem.RootElement.GetProperty("type").GetString().ShouldBe("urn:festos:problem:idempotency-key-missing");
    }

    [Theory]
    [InlineData("truss-1")]
    [InlineData("{0192f0a0-0000-7000-8000-000000000001}")]
    [InlineData("0192f0a0-0000-7000-8000-000000000001, 0192f0a0-0000-7000-8000-000000000002")]
    public async Task Post_WithAKeyThatIsNotOneUuid_IsMalformed(string key)
    {
        using HttpResponseMessage response = await PostAsync("/api/v1/things", """{ "name": "Truss" }""", key);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CodeOfAsync(response)).ShouldBe("malformedRequest");
    }

    [Fact]
    public async Task Post_WithAKey_PutsItInTheRequestAndStillBindsTheBody()
    {
        SeenRequest seen = await SeeAsync("/api/v1/things", """{ "name": "Truss" }""");

        seen.Key.ShouldBe(Guid.Parse(Key));
        seen.Fingerprint.ShouldNotBeNull().Length.ShouldBe(64);
        seen.Name.ShouldBe("Truss");
    }

    [Theory]
    [InlineData("/api/v1/things", """{ "name": "Truss" }""", true)]
    [InlineData("/api/v1/things", """{ "name": "Stage" }""", false)]
    [InlineData("/api/v1/things", """{"name":"Truss"}""", false)]
    [InlineData("/api/v1/things?copy=true", """{ "name": "Truss" }""", false)]
    public async Task Fingerprint_IsTheSameOnlyForTheSameRequest(string path, string body, bool same)
    {
        SeenRequest first = await SeeAsync("/api/v1/things", """{ "name": "Truss" }""");

        SeenRequest second = await SeeAsync(path, body);

        string.Equals(second.Fingerprint, first.Fingerprint, StringComparison.Ordinal).ShouldBe(same);
    }

    [Fact]
    public async Task Put_WithoutAnyHeader_ChecksTheKeyBeforeTheVersion()
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, new Uri("/api/v1/things/1", UriKind.Relative));
        using HttpResponseMessage response = await Client.SendAsync(request, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CodeOfAsync(response)).ShouldBe("idempotencyKeyMissing");
    }

    private async Task<SeenRequest> SeeAsync(string path, string body)
    {
        using HttpResponseMessage response = await PostAsync(path, body, Key);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SeenRequest>(Cancellation)).ShouldNotBeNull();
    }

    private async Task<HttpResponseMessage> PostAsync(string path, string body, string? key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(path, UriKind.Relative))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        if (key is not null)
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", key).ShouldBeTrue();
        }

        return await Client.SendAsync(request, Cancellation);
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return problem.RootElement.GetProperty("code").GetString();
    }

    public sealed record ThingRequest(string Name);

    public sealed record SeenRequest(Guid? Key, string? Fingerprint, string Name);
}
