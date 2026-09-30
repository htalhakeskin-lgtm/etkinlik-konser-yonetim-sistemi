using System.Diagnostics.Metrics;
using System.Security.Claims;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.UnitTests.Realtime;

/// <summary>Joining notification groups through a real SignalR connection, in memory (building-blocks §11).</summary>
public sealed class NotificationsHubTests : IAsyncLifetime
{
    private WebApplication? _app;
    private HubConnection? _connection;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private HubConnection Connection => _connection!;

    public async ValueTask InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddHttpPlatform();
        builder.AddRealtime();
        builder.Services.AddRealtimeGroup<ThingsPolicy>();
        _app = builder.Build();
        _app.UseHttpPlatform();
        _app.MapRealtime();
        await _app.StartAsync(Cancellation);

        _connection = Connect(_app);
        await _connection.StartAsync(Cancellation);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("things")]
    [InlineData("things:allowed")]
    public async Task JoinGroup_TheModulePolicyAllows_Joins(string group) =>
        await Should.NotThrowAsync(() => Connection.InvokeAsync("JoinGroup", group, Cancellation));

    [Fact]
    public async Task JoinGroup_TheModulePolicyRefuses_IsRejected()
    {
        HubException refused = await Should.ThrowAsync<HubException>(() =>
            Connection.InvokeAsync("JoinGroup", "things:hidden", Cancellation)
        );

        refused.Message.ShouldContain("may not join");
    }

    [Fact]
    public async Task JoinGroup_OfATypeNoModuleRegistered_IsRejected()
    {
        HubException refused = await Should.ThrowAsync<HubException>(() =>
            Connection.InvokeAsync("JoinGroup", "warehouses:1", Cancellation)
        );

        refused.Message.ShouldContain("No module");
    }

    [Theory]
    [InlineData("Things:1")]
    [InlineData("things:")]
    [InlineData("things:1:2")]
    [InlineData("")]
    public async Task JoinGroup_WithAMalformedName_IsRejected(string group)
    {
        HubException refused = await Should.ThrowAsync<HubException>(() =>
            Connection.InvokeAsync("JoinGroup", group, Cancellation)
        );

        refused.Message.ShouldContain("not a group name");
    }

    [Fact]
    public async Task Connections_AreCountedWhileOpen()
    {
        long open = 0;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument is { Name: "festos.realtime.connections", Meter.Name: MessagingMetrics.MeterName })
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, change, _, _) => Interlocked.Add(ref open, change));
        listener.Start();

        HubConnection second = Connect(_app!);
        await second.StartAsync(Cancellation);
        await WaitForAsync(() => Interlocked.Read(ref open) == 1);
        await second.DisposeAsync();

        await WaitForAsync(() => Interlocked.Read(ref open) == 0);
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(Cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        while (!condition())
        {
            await Task.Delay(TimeSpan.FromMilliseconds(20), timeout.Token);
        }
    }

    private static HubConnection Connect(WebApplication app)
    {
        TestServer server = app.GetTestServer();
        return new HubConnectionBuilder()
            .WithUrl(
                new Uri(server.BaseAddress, NotificationsHub.Path),
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                }
            )
            .Build();
    }

    private sealed class ThingsPolicy : IRealtimeGroupPolicy
    {
        public string GroupType => "things";

        public ValueTask<bool> CanJoinAsync(ClaimsPrincipal user, string? id, CancellationToken cancellationToken) =>
            ValueTask.FromResult(id is null or "allowed");
    }
}
