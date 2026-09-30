using System.Text.Json;
using FestOS.BuildingBlocks.Domain.Identifiers;
using FestOS.BuildingBlocks.Domain.Monetary;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.OpenApi;

/// <summary>The OpenAPI document of a few sample endpoints, through the real generator (api §14.1).</summary>
public sealed class OpenApiDocumentTests : IAsyncLifetime
{
    private WebApplication? _app;
    private string _json = string.Empty;
    private JsonElement _document;

    public enum ThingStatus
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
        builder.AddApiDocument();
        _app = builder.Build();
        _app.UseHttpPlatform();
        _app.MapOpenApi();
        _app.MapAntiforgeryToken();

        RouteGroupBuilder things = _app.MapGroup("/api/v1").RequiresIdempotencyKey().WithTags("Sample");
        things.MapGet("/things/{thingId:guid}", GetThing).WithName("GetThing").WithSummary("Gets a thing.");
        things.MapPost("/things", CreateThing).WithName("CreateThing").WithSummary("Creates a thing.");
        things
            .MapPut("/things/{thingId:guid}", RenameThing)
            .RequiresVersion()
            .WithName("RenameThing")
            .WithSummary("Renames a thing.");

        await _app.StartAsync(Cancellation);
        using HttpClient client = _app.GetTestClient();
        _json = (
            await client.GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative), Cancellation)
        ).ReplaceLineEndings("\n");
        _document = JsonDocument.Parse(_json).RootElement.Clone();
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public void Document_IsOpenApi31WithTheProjectTitle()
    {
        _document.GetProperty("openapi").GetString().ShouldStartWith("3.1");
        _document.GetProperty("info").GetProperty("title").GetString().ShouldBe("FestOS API");
    }

    [Fact]
    public void Text_IsAString_AndOnlyNullableTextAllowsNull()
    {
        Types(Property("ThingResponse", "name")).ShouldBe(["string"]);
        Types(Property("ThingResponse", "note")).ShouldBe(["null", "string"], ignoreOrder: true);
    }

    [Fact]
    public void Decimals_AreTextInTheDecimalFormat()
    {
        AssertDecimal(Property("ThingResponse", "price"), nullable: false);
        AssertDecimal(Property("ThingResponse", "discount"), nullable: true);
        AssertDecimal(Property("Money", "amount"), nullable: false);
    }

    [Fact]
    public void StronglyTypedIds_AreUuids()
    {
        Property("ThingResponse", "id").GetProperty("$ref").GetString().ShouldBe("#/components/schemas/ThingId");
        JsonElement id = Schema("ThingId");
        Types(id).ShouldBe(["string"]);
        id.GetProperty("format").GetString().ShouldBe("uuid");
        id.TryGetProperty("properties", out _).ShouldBeFalse();
    }

    [Fact]
    public void Enums_AreCamelCaseText()
    {
        JsonElement status = Schema("ThingStatus");
        Types(status).ShouldBe(["string"]);
        status.GetProperty("enum").EnumerateArray().Select(value => value.GetString()).ShouldBe(["draft", "inUse"]);
    }

    [Fact]
    public void DurationsAreWholeMinutes_AndCurrenciesTheirCode()
    {
        Types(Property("ThingResponse", "buffer")).ShouldBe(["integer"]);
        JsonElement currency = Schema("Currency");
        Types(currency).ShouldBe(["string"]);
        currency.GetProperty("pattern").GetString().ShouldBe("^[A-Z]{3}$");
    }

    [Fact]
    public void NullableArraysAndObjects_AllowNull()
    {
        Types(Property("ThingResponse", "tags")).ShouldBe(["null", "array"], ignoreOrder: true);
        Property("ThingResponse", "part")
            .GetProperty("oneOf")
            .EnumerateArray()
            .Select(option => option.TryGetProperty("type", out JsonElement type) ? type.GetString() : null)
            .Any(type => string.Equals(type, "null", StringComparison.Ordinal))
            .ShouldBeTrue();
    }

    [Fact]
    public void RequiredFields_FollowTheCSharpTypes() =>
        Schema("ThingResponse")
            .GetProperty("required")
            .EnumerateArray()
            .Select(field => field.GetString())
            .ShouldBe(
                [
                    "id",
                    "name",
                    "note",
                    "price",
                    "discount",
                    "status",
                    "previousStatus",
                    "buffer",
                    "fee",
                    "tags",
                    "part",
                    "version",
                ],
                ignoreOrder: true
            );

    [Fact]
    public void ChangingOperations_RequireAnIdempotencyKey_AndReadingOnesDoNot()
    {
        RequiredHeaders(Operation("/api/v1/things", "post")).ShouldBe(["Idempotency-Key"]);
        RequiredHeaders(Operation("/api/v1/things/{thingId}", "get")).ShouldBeEmpty();
    }

    [Fact]
    public void VersionedOperations_RequireIfMatch() =>
        RequiredHeaders(Operation("/api/v1/things/{thingId}", "put"))
            .ShouldBe(["If-Match", "Idempotency-Key"], ignoreOrder: true);

    [Fact]
    public void EveryOperation_DescribesItsErrorsAsProblemDetails()
    {
        foreach (
            (string path, string method) in new[]
            {
                ("/api/v1/things", "post"),
                ("/api/v1/things/{thingId}", "get"),
                ("/api/v1/things/{thingId}", "put"),
            }
        )
        {
            Operation(path, method)
                .GetProperty("responses")
                .GetProperty("default")
                .GetProperty("content")
                .GetProperty("application/problem+json")
                .GetProperty("schema")
                .GetProperty("$ref")
                .GetString()
                .ShouldBe("#/components/schemas/ApiProblem");
        }

        Schema("ApiProblem")
            .GetProperty("required")
            .EnumerateArray()
            .Select(field => field.GetString())
            .ShouldBe(["type", "title", "status", "detail", "instance", "code", "traceId"], ignoreOrder: true);
    }

    [Fact]
    public void AntiforgeryTokenEndpoint_IsLeftToTheRequestWrapper() =>
        _document.GetProperty("paths").TryGetProperty("/api/v1/antiforgery", out _).ShouldBeFalse();

    // The front end generates a client from this document and type-checks it, so a format its generator
    // cannot handle fails the build (api §14.1). The test rewrites a stale copy; commit the new one.
    [Fact]
    public async Task ContractDocument_IsTheCommittedCopyTheFrontEndChecks()
    {
        string path = Path.Combine(RepositoryRoot(), "src", "web", "openapi", "contract.json");
        string? committed = File.Exists(path) ? await File.ReadAllTextAsync(path, Cancellation) : null;

        if (!string.Equals(committed, _json, StringComparison.Ordinal))
        {
            await File.WriteAllTextAsync(path, _json, Cancellation);
            Assert.Fail($"{path} was out of date and has been rewritten; commit it.");
        }
    }

    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FestOS.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
    }

    private JsonElement Schema(string name) =>
        _document.GetProperty("components").GetProperty("schemas").GetProperty(name);

    private JsonElement Property(string schema, string property) =>
        Schema(schema).GetProperty("properties").GetProperty(property);

    private JsonElement Operation(string path, string method) =>
        _document.GetProperty("paths").GetProperty(path).GetProperty(method);

    private static string?[] Types(JsonElement schema)
    {
        JsonElement type = schema.GetProperty("type");
        return type.ValueKind == JsonValueKind.Array
            ? [.. type.EnumerateArray().Select(value => value.GetString())]
            : [type.GetString()];
    }

    private static void AssertDecimal(JsonElement schema, bool nullable)
    {
        Types(schema).ShouldBe(nullable ? ["null", "string"] : ["string"], ignoreOrder: true);
        schema.GetProperty("format").GetString().ShouldBe("decimal");
    }

    private static string?[] RequiredHeaders(JsonElement operation) =>
        operation.TryGetProperty("parameters", out JsonElement parameters)
            ?
            [
                .. parameters
                    .EnumerateArray()
                    .Where(parameter =>
                        string.Equals(parameter.GetProperty("in").GetString(), "header", StringComparison.Ordinal)
                        && parameter.GetProperty("required").GetBoolean()
                    )
                    .Select(parameter => parameter.GetProperty("name").GetString()),
            ]
            : [];

    private static Ok<ThingResponse> GetThing(Guid thingId) => throw new NotSupportedException();

    private static Created<ThingCreated> CreateThing(CreateThingRequest request) => throw new NotSupportedException();

    private static NoContent RenameThing(Guid thingId, RenameThingRequest request) => throw new NotSupportedException();

    public readonly record struct ThingId(Guid Value) : IStronglyTypedId<ThingId>
    {
        public static ThingId From(Guid value) => new(value);
    }

    public sealed record ThingPart(string Label);

    public sealed record ThingResponse(
        ThingId Id,
        string Name,
        string? Note,
        decimal Price,
        decimal? Discount,
        ThingStatus Status,
        ThingStatus? PreviousStatus,
        TimeSpan Buffer,
        Money Fee,
        IReadOnlyList<string>? Tags,
        ThingPart? Part,
        int Version
    );

    public sealed record CreateThingRequest(string Name, decimal Price, ThingStatus Status);

    public sealed record ThingCreated(ThingId Id);

    public sealed record RenameThingRequest(string Name);
}
