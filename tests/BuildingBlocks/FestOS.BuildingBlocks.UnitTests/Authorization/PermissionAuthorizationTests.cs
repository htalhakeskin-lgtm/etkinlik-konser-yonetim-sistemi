using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.UnitTests.Authorization;

/// <summary>Endpoints that require a permission, through the real pipeline (security §3.3).</summary>
[Trait("Rule", "BR-SYS-002")]
public sealed class PermissionAuthorizationTests : IAsyncLifetime
{
    private const string ViewThings = "Sample.Things.View";
    private const string PermissionsHeader = "X-Test-Permissions";

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
        builder.Services.AddSingleton(new ModuleCatalog([new SampleModule()]));
        builder.Services.AddPermissionAuthorization();
        builder
            .Services.AddAuthentication(TestAuthentication.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthentication>(TestAuthentication.SchemeName, _ => { });
        _app = builder.Build();
        _app.UseHttpPlatform();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapGet("/api/v1/things", () => "listed").RequirePermission(ViewThings);
        _app.MapGet("/api/v1/typo", () => "listed").RequirePermission("Sample.Thngs.View");
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
    public async Task UserWithThePermission_IsServed()
    {
        using HttpResponseMessage response = await GetAsync("/api/v1/things", "Other.Permission," + ViewThings);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UserWithoutThePermission_IsForbidden()
    {
        using HttpResponseMessage response = await GetAsync("/api/v1/things", "Other.Permission");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(response)).ShouldBe("forbidden");
    }

    [Fact]
    public async Task RequestWithoutASignedInUser_IsUnauthorized()
    {
        using HttpResponseMessage response = await GetAsync("/api/v1/things", permissions: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await CodeOfAsync(response)).ShouldBe("unauthorized");
    }

    [Fact]
    public async Task PermissionOutsideTheCatalog_FailsInsteadOfPassing()
    {
        using HttpResponseMessage response = await GetAsync("/api/v1/typo", "Sample.Thngs.View");

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    private async Task<HttpResponseMessage> GetAsync(string path, string? permissions)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
        if (permissions is not null)
        {
            request.Headers.Add(PermissionsHeader, permissions);
        }

        return await Client.SendAsync(request, Cancellation);
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return problem.RootElement.GetProperty("code").GetString();
    }

    private sealed class SampleModule : IModuleDefinition
    {
        public string Name => "Sample";

        public string Schema => "sample";

        public IReadOnlyCollection<string> Permissions => [ViewThings];

        public void RegisterServices(IHostApplicationBuilder builder) { }

        public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
    }

    // Signs in whoever sends the header, with the permissions it lists.
    private sealed class TestAuthentication(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (
                !Request.Headers.TryGetValue(PermissionsHeader, out Microsoft.Extensions.Primitives.StringValues header)
            )
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            Claim[] claims =
            [
                .. header.ToString().Split(',').Select(permission => new Claim(PermissionClaims.Type, permission)),
            ];
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
        }
    }
}
