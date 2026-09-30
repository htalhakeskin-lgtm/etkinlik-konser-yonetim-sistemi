using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>Idempotency keys end to end: the header, the unit of work and the replayed response (api §9, §10).</summary>
public sealed class IdempotencyHttpTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _client;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HttpClient Client => _client!;

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetAsync();
        _app = await fixture.StartWebApplicationAsync();
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
    public async Task Post_RetriedWithTheSameKey_CreatesOnceAndReturnsTheSameResponse()
    {
        var key = Guid.CreateVersion7();

        using HttpResponseMessage first = await CreateAsync("Stage", key);
        using HttpResponseMessage retry = await CreateAsync("Stage", key);

        first.StatusCode.ShouldBe(HttpStatusCode.Created);
        first.Headers.Contains("Idempotency-Replayed").ShouldBeFalse();
        retry.StatusCode.ShouldBe(HttpStatusCode.Created);
        retry.Headers.GetValues("Idempotency-Replayed").ShouldHaveSingleItem().ShouldBe("true");
        retry.Headers.Location.ShouldBe(first.Headers.Location);
        (await retry.Content.ReadAsStringAsync(Cancellation)).ShouldBe(
            await first.Content.ReadAsStringAsync(Cancellation)
        );
        (await CountItemsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Post_WithTheSameKeyForAnotherBody_IsRejected()
    {
        var key = Guid.CreateVersion7();
        using HttpResponseMessage first = await CreateAsync("Stage", key);

        using HttpResponseMessage other = await CreateAsync("Truss", key);

        other.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        using var problem = JsonDocument.Parse(await other.Content.ReadAsStringAsync(Cancellation));
        problem.RootElement.GetProperty("code").GetString().ShouldBe("idempotencyKeyReused");
        (await CountItemsAsync()).ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-011")]
    public async Task Put_RetriedAfterItSucceeded_ReturnsTheSameResponseInsteadOfAConflict()
    {
        using HttpResponseMessage created = await CreateAsync("Stage", Guid.CreateVersion7());
        Uri use = new($"{created.Headers.Location}/use", UriKind.Relative);
        var key = Guid.CreateVersion7();

        using HttpResponseMessage first = await UseAsync(use, key, ifMatch: "\"1\"");
        using HttpResponseMessage retry = await UseAsync(use, key, ifMatch: "\"1\"");
        using HttpResponseMessage stale = await UseAsync(use, Guid.CreateVersion7(), ifMatch: "\"1\"");

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        retry.StatusCode.ShouldBe(HttpStatusCode.NoContent, "the retry gets the stored result, not a false conflict");
        retry.Headers.GetValues("Idempotency-Replayed").ShouldHaveSingleItem().ShouldBe("true");
        stale.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed, "a new request based on version 1 is stale");
    }

    private async Task<HttpResponseMessage> CreateAsync(string name, Guid key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/v1/sample-items", UriKind.Relative))
        {
            Content = JsonContent.Create(new { name, unitPrice = "10.00" }),
        };
        request.Headers.Add("Idempotency-Key", key.ToString());
        return await Client.SendAsync(request, Cancellation);
    }

    private async Task<HttpResponseMessage> UseAsync(Uri address, Guid key, string ifMatch)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, address);
        request.Headers.Add("Idempotency-Key", key.ToString());
        request.Headers.TryAddWithoutValidation("If-Match", ifMatch).ShouldBeTrue();
        return await Client.SendAsync(request, Cancellation);
    }

    private async Task<int> CountItemsAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<SampleDbContext>().SampleItems.CountAsync(Cancellation);
    }
}
