using System.Diagnostics.Metrics;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>Per-record ordering and the delivery metrics (building-blocks §7, BB-04, observability §5).</summary>
public sealed class EventOrderingTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private SampleListenerProbe Probe => fixture.Services.GetRequiredService<SampleListenerProbe>();

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ProcessBatch_ForEventsOfOneRecord_KeepsTheLaterOneWaitingUntilTheEarlierIsDelivered()
    {
        SampleItemId id = await CreateItemAsync("Stage");
        await UseAsync(id);
        await UseAsync(id);
        Probe.FailuresRemaining = 1;

        (await ProcessAsync()).ShouldBe(1, "only the earlier event is taken; it fails");
        fixture.Time.Advance(TimeSpan.FromSeconds(5));
        (await ProcessAsync()).ShouldBe(1, "the earlier event is retried and delivered, the later still waits");
        (await ProcessAsync()).ShouldBe(1, "now the later event goes");

        List<OutboxMessage> messages = await OutboxAsync();
        messages.ShouldAllBe(message => message.DispatchedAt != null);
        messages[1].DispatchedAt!.Value.ShouldBeGreaterThanOrEqualTo(messages[0].DispatchedAt!.Value);
        (await UsageRecordCountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task ProcessBatch_WhileOneRecordsEventFails_DeliversTheEventsOfOtherRecords()
    {
        await UseAsync(await CreateItemAsync("Stage"));
        await UseAsync(await CreateItemAsync("Other stage"));
        Probe.FailuresRemaining = 1;

        (await ProcessAsync()).ShouldBe(2);

        List<OutboxMessage> messages = await OutboxAsync();
        messages[0].DispatchedAt.ShouldBeNull();
        messages[0].AttemptCount.ShouldBe(1);
        messages[1].DispatchedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ProcessBatch_ForDeliveredAndFailedEvents_RecordsLatencyAndDeadLetters()
    {
        List<double> latencies = [];
        long deadLetters = 0;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (string.Equals(instrument.Meter.Name, MessagingMetrics.MeterName, StringComparison.Ordinal))
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<double>(
            (instrument, value, _, _) =>
            {
                if (string.Equals(instrument.Name, "festos.messaging.event.latency", StringComparison.Ordinal))
                {
                    latencies.Add(value);
                }
            }
        );
        listener.SetMeasurementEventCallback<long>(
            (instrument, value, _, _) =>
            {
                if (string.Equals(instrument.Name, "festos.messaging.dead_letters", StringComparison.Ordinal))
                {
                    deadLetters += value;
                }
            }
        );
        listener.Start();

        await UseAsync(await CreateItemAsync("Stage"));
        fixture.Time.Advance(TimeSpan.FromSeconds(2));
        await ProcessAsync();

        latencies.ShouldHaveSingleItem().ShouldBe(2d);

        await UseAsync(await CreateItemAsync("Other stage"));
        Probe.FailuresRemaining = int.MaxValue;
        for (int attempt = 1; attempt <= OutboxProcessor.MaxAttempts; attempt++)
        {
            await ProcessAsync();
            fixture.Time.Advance(TimeSpan.FromHours(1));
        }

        deadLetters.ShouldBe(1);
    }

    private Task<int> ProcessAsync() =>
        fixture
            .Services.GetRequiredService<OutboxProcessor>()
            .ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

    private async Task<SampleItemId> CreateItemAsync(string name)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var item = SampleItem.Create(name, 1m);
        context.SampleItems.Add(item);
        await context.SaveChangesAsync(Cancellation);
        return item.Id;
    }

    private async Task UseAsync(SampleItemId id)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        (
            await context
                .SampleItems.Include(sample => sample.Parts)
                .SingleAsync(sample => sample.Id == id, Cancellation)
        ).Use();
        await context.SaveChangesAsync(Cancellation);
    }

    private async Task<List<OutboxMessage>> OutboxAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<SampleDbContext>()
            .Set<OutboxMessage>()
            .OrderBy(message => message.Sequence)
            .ToListAsync(Cancellation);
    }

    private async Task<int> UsageRecordCountAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<SampleDbContext>()
            .Set<SampleUsageRecord>()
            .CountAsync(Cancellation);
    }
}
