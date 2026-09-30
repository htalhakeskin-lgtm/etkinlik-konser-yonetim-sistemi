using System.Diagnostics;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>
/// The background dispatcher of a running host (building-blocks §7, 05 §9.2). The started host has a
/// fake clock that the test does not move, so only a signal, the startup pass or an explicit clock move
/// can wake the dispatcher.
/// </summary>
public sealed class DispatcherTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(15);

    private readonly FakeTimeProvider _hostTime = new(new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero));

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Dispatcher_WhenACommandCommitsAnEvent_DeliversItWithoutWaitingForThePoll()
    {
        using IHost host = fixture.CreateHost(_hostTime);
        await host.StartAsync(Cancellation);

        SampleItemId id = await SendAsync<CreateSampleItemCommand, SampleItemId>(host, new("Stage", 1m));
        await SendAsync<UseSampleItemCommand, bool>(host, new(id));

        await WaitForUsageRecordAsync(host, "the commit's signal wakes the dispatcher");
        await host.StopAsync(Cancellation);
    }

    [Fact]
    public async Task Dispatcher_OnStartup_DeliversWhatWasAlreadyWaiting()
    {
        await UseItemWithoutSignalAsync(fixture.Services);
        using IHost host = fixture.CreateHost(_hostTime);

        await host.StartAsync(Cancellation);

        await WaitForUsageRecordAsync(host, "the first pass runs on startup");
        await host.StopAsync(Cancellation);
    }

    [Fact]
    public async Task Dispatcher_WithoutASignal_DeliversAtTheNextPoll()
    {
        // The first event, delivered by the startup pass, shows the dispatcher has started its loop.
        await UseItemWithoutSignalAsync(fixture.Services);
        using IHost host = fixture.CreateHost(_hostTime);
        await host.StartAsync(Cancellation);
        await WaitForUsageRecordsAsync(host, 1, "the startup pass");

        await UseItemWithoutSignalAsync(host.Services, "Second stage");

        // Moving the clock at every check fires the poll whenever the dispatcher starts waiting.
        await WaitForUsageRecordsAsync(
            host,
            2,
            "the poll interval passed",
            beforeCheck: () => _hostTime.Advance(TimeSpan.FromSeconds(5))
        );
        await host.StopAsync(Cancellation);
    }

    [Fact]
    public async Task Delivery_OfAnEvent_ContinuesTheTraceOfTheRequestThatRaisedIt()
    {
        List<Activity> deliveries = [];
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => string.Equals(source.Name, "FestOS.BuildingBlocks", StringComparison.Ordinal),
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                if (activity.DisplayName.StartsWith("Deliver ", StringComparison.Ordinal))
                {
                    deliveries.Add(activity);
                }
            },
        };
        ActivitySource.AddActivityListener(listener);

        ActivityTraceId requestTrace;
        using (Activity request = new Activity("request").SetIdFormat(ActivityIdFormat.W3C).Start())
        {
            requestTrace = request.TraceId;
            await UseItemWithoutSignalAsync(fixture.Services);
        }

        await fixture
            .Services.GetRequiredService<OutboxProcessor>()
            .ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        Activity delivery = deliveries.ShouldHaveSingleItem();
        delivery.DisplayName.ShouldBe(
            $"Deliver {nameof(FestOS.Modules.Sample.IntegrationEvents.SampleItemUsedIntegrationEvent)}"
        );
        delivery.TraceId.ShouldBe(requestTrace);
    }

    // A direct save writes the event but, unlike a command, does not signal the dispatcher.
    private static async Task UseItemWithoutSignalAsync(IServiceProvider services, string name = "Stage")
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var item = SampleItem.Create(name, 1m);
        item.Use();
        context.SampleItems.Add(item);
        await context.SaveChangesAsync(Cancellation);
    }

    private static async Task<TResult> SendAsync<TCommand, TResult>(IHost host, TCommand command)
        where TCommand : ICommand<TResult>
    {
        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>()
            .HandleAsync(command, Cancellation);
    }

    private static Task WaitForUsageRecordAsync(IHost host, string because) =>
        WaitForUsageRecordsAsync(host, 1, because);

    private static Task WaitForUsageRecordsAsync(IHost host, int count, string because, Action? beforeCheck = null) =>
        Eventually.WaitUntilAsync(
            async () =>
            {
                beforeCheck?.Invoke();
                await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
                return await scope
                        .ServiceProvider.GetRequiredService<SampleDbContext>()
                        .Set<SampleUsageRecord>()
                        .CountAsync(Cancellation) == count;
            },
            DeliveryTimeout,
            because
        );
}
