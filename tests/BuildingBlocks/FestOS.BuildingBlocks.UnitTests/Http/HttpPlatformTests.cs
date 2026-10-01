using System.Net;
using System.Text;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.Http;

/// <summary>Error responses and API headers through the real middleware, in memory (api §8, §11, §12).</summary>
public sealed class HttpPlatformTests : IAsyncLifetime
{
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

        _app.MapGet(
            "/api/v1/rule",
            string () =>
                throw new BusinessRuleViolationException(
                    "SAMPLE-001",
                    "Sample rule violated.",
                    parameters: new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["performedBy"] = "Sample User",
                    }
                )
        );
        _app.MapGet(
            "/api/v1/authorization-rule",
            string () =>
                throw new BusinessRuleViolationException("SAMPLE-002", "Only the owner.", RuleKind.Authorization)
        );
        _app.MapGet("/api/v1/missing", string () => throw new NotFoundException("Sample", Guid.Empty));
        _app.MapGet("/api/v1/stale", string () => throw new ConcurrencyConflictException("Sample changed."));
        _app.MapGet("/api/v1/reused-key", string () => throw new IdempotencyKeyReusedException("Other request."));
        _app.MapGet("/api/v1/key-in-progress", string () => throw new IdempotencyKeyInProgressException("Running."));
        _app.MapGet("/api/v1/limited", string () => throw new RateLimitedException(TimeSpan.FromSeconds(41.2)));
        _app.MapGet(
            "/api/v1/invalid",
            string () =>
                throw new ValidationFailedException([
                    new ValidationError("Name", "NotEmptyValidator", Parameters()),
                    new ValidationError(
                        "Units[3].SerialNumber",
                        "MaximumLengthValidator",
                        Parameters(("MaxLength", 20), ("TotalLength", 25))
                    ),
                    new ValidationError("Sort", "unsupportedSort", Parameters(("AllowedFields", "name"))),
                    new ValidationError("Code", "PredicateValidator", Parameters()),
                ])
        );
        _app.MapGet("/api/v1/broken", string () => throw new InvalidOperationException("Secret internal detail."));
        _app.MapPost("/api/v1/body", (SampleBody body) => body.Name);
        _app.MapGet("/api/v1/ok", () => "ok");
        _app.MapGet("/health-like", () => "ok");

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
    public async Task BusinessRuleViolation_BecomesA422WithTheRuleCodeAndParameters()
    {
        (HttpResponseMessage response, JsonElement problem) = await GetProblemAsync("/api/v1/rule");

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        problem.GetProperty("type").GetString().ShouldBe("urn:festos:problem:business-rule");
        problem.GetProperty("code").GetString().ShouldBe("SAMPLE-001");
        problem.GetProperty("params").GetProperty("performedBy").GetString().ShouldBe("Sample User");
        problem.GetProperty("instance").GetString().ShouldBe("GET /api/v1/rule");
        problem.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("/api/v1/authorization-rule", HttpStatusCode.Forbidden, "SAMPLE-002")]
    [InlineData("/api/v1/missing", HttpStatusCode.NotFound, "notFound")]
    [InlineData("/api/v1/stale", HttpStatusCode.PreconditionFailed, "concurrencyConflict")]
    [InlineData("/api/v1/reused-key", HttpStatusCode.UnprocessableEntity, "idempotencyKeyReused")]
    [InlineData("/api/v1/key-in-progress", HttpStatusCode.Conflict, "idempotencyKeyInProgress")]
    [InlineData("/api/v1/limited", HttpStatusCode.TooManyRequests, "rateLimited")]
    [InlineData("/api/v1/unknown-address", HttpStatusCode.NotFound, "notFound")]
    public async Task ExpectedOutcomes_BecomeTheirStatusAndCode(string path, HttpStatusCode status, string code)
    {
        (HttpResponseMessage response, JsonElement problem) = await GetProblemAsync(path);

        response.StatusCode.ShouldBe(status);
        problem.GetProperty("code").GetString().ShouldBe(code);
    }

    [Fact]
    public async Task RateLimited_SaysWhenToTryAgainInWholeSeconds()
    {
        using HttpResponseMessage response = await _client!.GetAsync(
            new Uri("/api/v1/limited", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter!.Delta.ShouldBe(TimeSpan.FromSeconds(42));
    }

    [Fact]
    public async Task UnknownApiAddress_UsesTheProjectsProblemType()
    {
        (_, JsonElement problem) = await GetProblemAsync("/api/v1/unknown-address");

        problem.GetProperty("type").GetString().ShouldBe("urn:festos:problem:not-found");
    }

    [Fact]
    public async Task ValidationFailure_ListsEachFieldAsAPointerWithItsCodeAndParameters()
    {
        (HttpResponseMessage response, JsonElement problem) = await GetProblemAsync("/api/v1/invalid");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.GetProperty("code").GetString().ShouldBe("validation");
        JsonElement[] errors = [.. problem.GetProperty("errors").EnumerateArray()];
        errors
            .Select(error => error.GetProperty("pointer").GetString())
            .ShouldBe(["/name", "/units/3/serialNumber", "/sort", "/code"]);
        errors
            .Select(error => error.GetProperty("code").GetString())
            .ShouldBe(["required", "maxLength", "unsupportedSort", "invalid"]);
        errors[1].GetProperty("params").GetProperty("max").GetInt32().ShouldBe(20);
        errors[1]
            .GetProperty("params")
            .TryGetProperty("totalLength", out _)
            .ShouldBeFalse("the submitted value's length stays out");
        errors[2].GetProperty("params").GetProperty("allowedFields").GetString().ShouldBe("name");
    }

    [Fact]
    public async Task UnexpectedError_BecomesA500WithoutItsDetails()
    {
        (HttpResponseMessage response, JsonElement problem) = await GetProblemAsync("/api/v1/broken");

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        problem.GetProperty("code").GetString().ShouldBe("internalError");
        problem.GetProperty("detail").GetString().ShouldBe("An unexpected error occurred.");
        problem.GetRawText().ShouldNotContain("Secret internal detail.");
    }

    [Fact]
    public async Task MalformedBody_BecomesA400MalformedRequest()
    {
        using var content = new StringContent("{ not json", Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await Client.PostAsync(
            new Uri("/api/v1/body", UriKind.Relative),
            content,
            Cancellation
        );
        JsonElement problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation)).RootElement;

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.GetProperty("code").GetString().ShouldBe("malformedRequest");
    }

    [Fact]
    public async Task ApiResponses_CarryTheVersionAndAreNeitherCachedNorSniffed()
    {
        using HttpResponseMessage response = await Client.GetAsync(
            new Uri("/api/v1/ok", UriKind.Relative),
            Cancellation
        );

        response.Headers.GetValues("X-App-Version").ShouldHaveSingleItem().ShouldBe(HttpPlatformExtensions.AppVersion);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
        response.Headers.GetValues("X-Content-Type-Options").ShouldHaveSingleItem().ShouldBe("nosniff");
    }

    [Fact]
    public async Task OtherResponses_DoNotGetTheApiHeaders()
    {
        using HttpResponseMessage response = await Client.GetAsync(
            new Uri("/health-like", UriKind.Relative),
            Cancellation
        );

        response.Headers.Contains("X-App-Version").ShouldBeFalse();
    }

    private static Dictionary<string, object?> Parameters(params (string Key, object? Value)[] values) =>
        values.ToDictionary(value => value.Key, value => value.Value, StringComparer.Ordinal);

    private async Task<(HttpResponseMessage Response, JsonElement Problem)> GetProblemAsync(string path)
    {
        HttpResponseMessage response = await Client.GetAsync(new Uri(path, UriKind.Relative), Cancellation);
        JsonElement problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation)).RootElement;
        return (response, problem);
    }

    public sealed record SampleBody(string Name);
}
