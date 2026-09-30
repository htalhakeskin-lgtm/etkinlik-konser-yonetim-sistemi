using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>
/// A change made through the API reaches the open screens of the changed record: the event goes through
/// the outbox and the dispatcher to the notification hub (ADR-0012, api §13).
/// </summary>
[Trait("Rule", "BR-SYS-012")]
public sealed class RealtimeTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ChangeThroughTheApi_IsNotifiedToTheGroupOfTheRecordOnly()
    {
        await using WebApplication app = await fixture.StartWebApplicationAsync();
        using HttpClient client = SampleModuleFixture.CreateClient(app);
        using HttpResponseMessage created = await client.PostAsJsonAsync(
            new Uri("/api/v1/sample-items", UriKind.Relative),
            new { name = "Stage", unitPrice = "10.00" },
            Cancellation
        );
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        Guid id = (await created.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("id").GetGuid();

        (HubConnection watcher, Task<JsonElement> notified) = await WatchAsync(app, $"sample-items:{id}");
        (HubConnection other, Task<JsonElement> otherNotified) = await WatchAsync(
            app,
            $"sample-items:{Guid.CreateVersion7()}"
        );
        await using (watcher)
        await using (other)
        {
            using var use = new HttpRequestMessage(
                HttpMethod.Put,
                new Uri($"/api/v1/sample-items/{id}/use", UriKind.Relative)
            );
            use.Headers.TryAddWithoutValidation("If-Match", "\"1\"").ShouldBeTrue();
            using HttpResponseMessage used = await client.SendAsync(use, Cancellation);
            used.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            JsonElement message = await notified.WaitAsync(TimeSpan.FromSeconds(10), Cancellation);
            message.GetProperty("resource").GetString().ShouldBe("sample-items");
            message.GetProperty("id").GetGuid().ShouldBe(id);
            message
                .GetProperty("version")
                .ValueKind.ShouldBe(JsonValueKind.Null, "the sample event carries no version");

            // Both groups are told at the same moment; give the other one time to prove it hears nothing.
            await Task.Delay(TimeSpan.FromMilliseconds(300), Cancellation);
            otherNotified.IsCompleted.ShouldBeFalse();
        }
    }

    private static async Task<(HubConnection Connection, Task<JsonElement> Notified)> WatchAsync(
        WebApplication app,
        string group
    )
    {
        TestServer server = app.GetTestServer();
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(
                new Uri(server.BaseAddress, NotificationsHub.Path),
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                }
            )
            .Build();
        var notified = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<JsonElement>(NotificationsHub.ResourceChangedMethod, message => notified.TrySetResult(message));
        await connection.StartAsync(Cancellation);
        await connection.InvokeAsync("JoinGroup", group, Cancellation);
        return (connection, notified.Task);
    }
}
