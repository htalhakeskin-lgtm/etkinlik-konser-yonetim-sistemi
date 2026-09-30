using System.Net;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.Http;

/// <summary>ETag and If-Match through the real pipeline, in memory (api §9).</summary>
[Trait("Rule", "BR-SYS-011")]
public sealed class IfMatchTests : IAsyncLifetime
{
    private const int StoredVersion = 7;
    private const string ItemPath = "/api/v1/items/1";

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

        _app.MapGet(ItemPath, () => TypedResults.Ok(new ItemResponse(StoredVersion)).WithVersion(StoredVersion));
        _app.MapPut(
                ItemPath,
                (ExpectedVersion expected) =>
                {
                    // What a handler does right after loading the aggregate it changes.
                    if (expected.Value != StoredVersion)
                    {
                        throw new ConcurrencyConflictException("Item was changed by someone else.");
                    }

                    return TypedResults.Ok(new ItemResponse(StoredVersion + 1)).WithVersion(StoredVersion + 1);
                }
            )
            .RequiresVersion();

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
    public async Task Get_ReturnsTheVersionAsAStrongETag()
    {
        using HttpResponseMessage response = await Client.GetAsync(new Uri(ItemPath, UriKind.Relative), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
        response.Headers.ETag.Tag.ShouldBe("\"7\"");
        response.Headers.ETag.IsWeak.ShouldBeFalse();
    }

    [Fact]
    public async Task Put_WithTheCurrentVersion_ChangesTheItemAndReturnsTheNewETag()
    {
        using HttpResponseMessage response = await PutAsync("\"7\"");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag!.Tag.ShouldBe("\"8\"");
    }

    [Fact]
    public async Task Put_WithAnOldVersion_IsRejectedAsAConflict()
    {
        using HttpResponseMessage response = await PutAsync("\"6\"");

        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        (await CodeOfAsync(response)).ShouldBe("concurrencyConflict");
    }

    [Fact]
    public async Task Put_WithoutIfMatch_RequiresTheVersion()
    {
        using HttpResponseMessage response = await PutAsync(ifMatch: null);

        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionRequired);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        problem.RootElement.GetProperty("code").GetString().ShouldBe("versionRequired");
        problem.RootElement.GetProperty("type").GetString().ShouldBe("urn:festos:problem:version-required");
    }

    [Theory]
    [InlineData("*")]
    [InlineData("W/\"7\"")]
    [InlineData("7")]
    [InlineData("\"\"")]
    [InlineData("\"seven\"")]
    [InlineData("\"-7\"")]
    [InlineData("\"7\", \"8\"")]
    public async Task Put_WithAnythingButOneStrongVersionTag_IsMalformed(string ifMatch)
    {
        using HttpResponseMessage response = await PutAsync(ifMatch);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CodeOfAsync(response)).ShouldBe("malformedRequest");
    }

    [Fact]
    public void RequiresVersion_MarksTheEndpointForTheOpenApiDocument()
    {
        Endpoint put = _app!
            .Services.GetRequiredService<EndpointDataSource>()
            .Endpoints.Single(endpoint =>
                endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("PUT", StringComparer.Ordinal)
                == true
            );

        put.Metadata.GetMetadata<RequiresVersionMetadata>().ShouldNotBeNull();
    }

    private async Task<HttpResponseMessage> PutAsync(string? ifMatch)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, new Uri(ItemPath, UriKind.Relative));
        if (ifMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch).ShouldBeTrue();
        }

        return await Client.SendAsync(request, Cancellation);
    }

    private static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return problem.RootElement.GetProperty("code").GetString();
    }

    public sealed record ItemResponse(int Version);
}
