using System.Net;
using System.Text;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Domain.Identifiers;
using FestOS.BuildingBlocks.Domain.Monetary;
using FestOS.BuildingBlocks.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.Http;

/// <summary>The API's JSON rules and the front end's security headers, through the real pipeline (api §5, security §8).</summary>
public sealed class ApiJsonTests : IAsyncLifetime
{
    private const string ValidBody = """
        {
          "name": "  Cafe\u0301  ",
          "password": "  secret  ",
          "price": "12.500",
          "kind": "inUse",
          "quantity": null,
          "bufferMinutes": 90,
          "id": "0192f0a0-0000-7000-8000-000000000001"
        }
        """;

    private WebApplication? _app;
    private HttpClient? _client;

    public enum SampleKind
    {
        Draft,
        InUse,
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddHttpPlatform();
        _app = builder.Build();
        _app.UseHttpPlatform();
        _app.MapPost(
            "/api/v1/echo",
            (EchoRequest request) =>
                new EchoResponse(
                    request.Name,
                    request.Password,
                    request.Price,
                    request.Kind,
                    request.Quantity,
                    new Money(1250.00m, Currency.TurkishLira),
                    TimeSpan.FromMinutes(request.BufferMinutes),
                    request.Id,
                    Note: null
                )
        );
        _app.MapGet("/page", () => "page");
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
    public async Task ValidBody_IsNormalizedOnTheWayInAndWrittenInTheApiFormats()
    {
        (HttpStatusCode status, JsonElement body) = await PostAsync(ValidBody);

        status.ShouldBe(HttpStatusCode.OK);
        body.GetProperty("name").GetString().ShouldBe("Café", "trimmed and brought to NFC");
        body.GetProperty("password").GetString().ShouldBe("  secret  ", "sensitive text is left as sent");
        body.GetProperty("price").GetString().ShouldBe("12.500");
        body.GetProperty("kind").GetString().ShouldBe("inUse");
        body.GetProperty("fee").GetProperty("amount").GetString().ShouldBe("1250.00");
        body.GetProperty("fee").GetProperty("currency").GetString().ShouldBe("TRY");
        body.GetProperty("buffer").GetInt32().ShouldBe(90);
        body.GetProperty("id").GetString().ShouldBe("0192f0a0-0000-7000-8000-000000000001");
        body.GetProperty("note").ValueKind.ShouldBe(JsonValueKind.Null, "null fields are written, not left out");
    }

    [Theory]
    [InlineData(
        """{ "name": "A", "name": "B", "password": "p", "price": "1", "kind": "draft", "bufferMinutes": 0, "id": "0192f0a0-0000-7000-8000-000000000001" }"""
    )]
    [InlineData(
        """{ "name": "A", "password": "p", "price": "1", "kind": "draft", "bufferMinutes": 0, "id": "0192f0a0-0000-7000-8000-000000000001", "extra": 1 }"""
    )]
    [InlineData(
        """{ "NAME": "A", "password": "p", "price": "1", "kind": "draft", "bufferMinutes": 0, "id": "0192f0a0-0000-7000-8000-000000000001" }"""
    )]
    [InlineData(
        """{ "name": null, "password": "p", "price": "1", "kind": "draft", "bufferMinutes": 0, "id": "0192f0a0-0000-7000-8000-000000000001" }"""
    )]
    [InlineData(
        """{ "name": "A", "password": "p", "price": 1.5, "kind": "draft", "bufferMinutes": 0, "id": "0192f0a0-0000-7000-8000-000000000001" }"""
    )]
    [InlineData(
        """{ "name": "A", "password": "p", "price": "1", "kind": 1, "bufferMinutes": 0, "id": "0192f0a0-0000-7000-8000-000000000001" }"""
    )]
    public async Task BodyBreakingTheRules_IsRejectedAsMalformed(string json)
    {
        (HttpStatusCode status, JsonElement body) = await PostAsync(json);

        status.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("code").GetString().ShouldBe("malformedRequest");
    }

    [Fact]
    public async Task FrontEndResponses_CarryTheBrowserSecurityHeaders()
    {
        using HttpResponseMessage response = await _client!.GetAsync(new Uri("/page", UriKind.Relative), Cancellation);

        string policy = response.Headers.GetValues("Content-Security-Policy").ShouldHaveSingleItem();
        policy.ShouldContain("script-src 'self'");
        policy.ShouldContain("frame-ancestors 'none'");
        response.Headers.GetValues("Referrer-Policy").ShouldHaveSingleItem().ShouldBe("no-referrer");
        response.Headers.GetValues("Permissions-Policy").ShouldHaveSingleItem().ShouldContain("camera=(self)");
        response.Headers.GetValues("Cross-Origin-Opener-Policy").ShouldHaveSingleItem().ShouldBe("same-origin");
        response.Headers.GetValues("X-Content-Type-Options").ShouldHaveSingleItem().ShouldBe("nosniff");
    }

    [Fact]
    public async Task ApiResponses_DoNotCarryThePageHeaders()
    {
        using var content = new StringContent(ValidBody, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _client!.PostAsync(
            new Uri("/api/v1/echo", UriKind.Relative),
            content,
            Cancellation
        );

        response.Headers.Contains("Content-Security-Policy").ShouldBeFalse();
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> PostAsync(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _client!.PostAsync(
            new Uri("/api/v1/echo", UriKind.Relative),
            content,
            Cancellation
        );
        return (
            response.StatusCode,
            JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation)).RootElement
        );
    }

    public readonly record struct SampleId(Guid Value) : IStronglyTypedId<SampleId>
    {
        public static SampleId From(Guid value) => new(value);
    }

    public sealed record EchoRequest(
        string Name,
        [property: Sensitive] string Password,
        decimal Price,
        SampleKind Kind,
        int? Quantity,
        int BufferMinutes,
        SampleId Id
    );

    public sealed record EchoResponse(
        string Name,
        string Password,
        decimal Price,
        SampleKind Kind,
        int? Quantity,
        Money Fee,
        TimeSpan Buffer,
        SampleId Id,
        string? Note
    );
}
