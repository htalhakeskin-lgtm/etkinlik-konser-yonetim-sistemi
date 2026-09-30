using System.Net;
using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.Http;

/// <summary>The three layers against request forgery through the real pipeline, over HTTPS (api §11).</summary>
public sealed class CsrfProtectionTests : IAsyncLifetime
{
    private const string ThingsPath = "/api/v1/things";

    private WebApplication? _app;
    private HttpClient? _client;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Client => _client!;

    public async ValueTask InitializeAsync() =>
        (_app, _client) = await StartAsync(Environments.Production, "https://localhost/");

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task TokenEndpoint_IssuesAnHttpOnlyCookieTokenAndARequestTokenTheFrontEndCanRead()
    {
        using HttpResponseMessage response = await Client.GetAsync(
            new Uri(CsrfProtection.TokenPath, UriKind.Relative),
            Cancellation
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        string[] cookies = [.. response.Headers.GetValues("Set-Cookie")];
        string cookieToken = cookies
            .Where(cookie => cookie.StartsWith("__Host-festos_antiforgery=", StringComparison.Ordinal))
            .ShouldHaveSingleItem();
        string requestToken = cookies
            .Where(cookie => cookie.StartsWith("__Host-festos_xsrf=", StringComparison.Ordinal))
            .ShouldHaveSingleItem();
        foreach (string cookie in cookies)
        {
            cookie.ShouldContain("path=/", Case.Insensitive);
            cookie.ShouldContain("secure", Case.Insensitive);
            cookie.ShouldContain("samesite=strict", Case.Insensitive);
        }

        cookieToken.ShouldContain("httponly", Case.Insensitive);
        requestToken.ShouldNotContain("httponly", Case.Insensitive);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("same-origin")]
    [InlineData("same-site")]
    [InlineData("none")]
    public async Task ChangingRequest_WithTheToken_Passes(string? fetchSite)
    {
        (string cookie, string token) = await GetTokensAsync(Client);

        using HttpResponseMessage response = await PostAsync(cookie, token, fetchSite);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangingRequest_WithoutTheToken_IsRejected()
    {
        using HttpResponseMessage response = await PostAsync(cookie: null, token: null);

        await ShouldBeRejectedAsync(response);
    }

    [Fact]
    public async Task ChangingRequest_WithTheHeaderButWithoutTheCookie_IsRejected()
    {
        (_, string token) = await GetTokensAsync(Client);

        using HttpResponseMessage response = await PostAsync(cookie: null, token);

        await ShouldBeRejectedAsync(response);
    }

    [Fact]
    public async Task ChangingRequest_TheBrowserReportsFromAnotherSite_IsRejectedEvenWithTheToken()
    {
        (string cookie, string token) = await GetTokensAsync(Client);

        using HttpResponseMessage response = await PostAsync(cookie, token, fetchSite: "cross-site");

        await ShouldBeRejectedAsync(response);
    }

    [Fact]
    public async Task ReadingRequest_NeedsNoToken()
    {
        using HttpResponseMessage response = await Client.GetAsync(new Uri(ThingsPath, UriKind.Relative), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Development_OverPlainHttp_IssuesCookiesWithoutThePrefix()
    {
        (WebApplication app, HttpClient client) = await StartAsync(Environments.Development, "http://localhost/");
        await using (app)
        using (client)
        {
            using HttpResponseMessage response = await client.GetAsync(
                new Uri(CsrfProtection.TokenPath, UriKind.Relative),
                Cancellation
            );

            string[] cookies = [.. response.Headers.GetValues("Set-Cookie")];
            cookies.ShouldContain(cookie => cookie.StartsWith("festos_antiforgery=", StringComparison.Ordinal));
            cookies.ShouldContain(cookie => cookie.StartsWith("festos_xsrf=", StringComparison.Ordinal));
            cookies.ShouldAllBe(cookie => !cookie.Contains("secure", StringComparison.OrdinalIgnoreCase));

            (string cookie, string token) = await GetTokensAsync(client);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(ThingsPath, UriKind.Relative));
            request.Headers.Add("Cookie", cookie);
            request.Headers.Add(CsrfProtection.HeaderName, token);
            using HttpResponseMessage post = await client.SendAsync(request, Cancellation);
            post.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    private static async Task<(WebApplication App, HttpClient Client)> StartAsync(string environment, string address)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions { EnvironmentName = environment }
        );
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddHttpPlatform();
        WebApplication app = builder.Build();
        app.UseHttpPlatform();
        app.UseCsrfProtection();
        app.MapAntiforgeryToken();
        app.MapGet(ThingsPath, () => "listed");
        app.MapPost(ThingsPath, () => "done");
        await app.StartAsync(Cancellation);

        HttpClient client = app.GetTestClient();
        client.BaseAddress = new Uri(address);
        return (app, client);
    }

    // The cookie token as the browser sends it back, and the request token the front end reads.
    private static async Task<(string Cookie, string Token)> GetTokensAsync(HttpClient client)
    {
        using HttpResponseMessage response = await client.GetAsync(
            new Uri(CsrfProtection.TokenPath, UriKind.Relative),
            Cancellation
        );
        string[] cookies = [.. response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])];
        string cookieToken = cookies.Single(cookie => cookie.Contains("festos_antiforgery=", StringComparison.Ordinal));
        string requestToken = cookies.Single(cookie => cookie.Contains("festos_xsrf=", StringComparison.Ordinal));
        return (cookieToken, requestToken[(requestToken.IndexOf('=', StringComparison.Ordinal) + 1)..]);
    }

    private async Task<HttpResponseMessage> PostAsync(string? cookie, string? token, string? fetchSite = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(ThingsPath, UriKind.Relative));
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        if (token is not null)
        {
            request.Headers.Add(CsrfProtection.HeaderName, token);
        }

        if (fetchSite is not null)
        {
            request.Headers.Add("Sec-Fetch-Site", fetchSite);
        }

        return await Client.SendAsync(request, Cancellation);
    }

    private static async Task ShouldBeRejectedAsync(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        problem.RootElement.GetProperty("code").GetString().ShouldBe("csrfRejected");
    }
}
