using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>
/// Delivering outbox messages to the listeners, with the inbox and retries (building-blocks §7, BB-03,
/// BB-05). The processor is called directly so each step is deterministic.
/// </summary>
public sealed class EventDeliveryTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private SampleListenerProbe Probe => fixture.Services.GetRequiredService<SampleListenerProbe>();

    private OutboxProcessor Processor => fixture.Services.GetRequiredService<OutboxProcessor>();

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ProcessBatch_ForAPendingEvent_DeliversItToEveryListenerAndMarksItDispatched()
    {
        SampleItemId id = await UseNewItemAsync();

        int processed = await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        processed.ShouldBe(1);
        (await UsageRecordsAsync()).ShouldHaveSingleItem().SampleItemId.ShouldBe(id.Value);
        Probe.Calls.ShouldBe(1);
        OutboxMessage message = (await OutboxAsync()).ShouldHaveSingleItem();
        message.DispatchedAt.ShouldBe(fixture.Time.GetUtcNow());
        (await InboxAsync()).Count.ShouldBe(2, "each listener records the event in its inbox");
    }

    [Fact]
    public async Task ProcessBatch_AfterDelivery_DoesNotDeliverAgain()
    {
        await UseNewItemAsync();
        await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        int processed = await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        processed.ShouldBe(0);
        Probe.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task ProcessBatch_WhenOneListenerFails_RetriesLaterWithoutRepeatingTheOthers()
    {
        await UseNewItemAsync();
        Probe.FailuresRemaining = 1;

        await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        OutboxMessage failed = (await OutboxAsync()).ShouldHaveSingleItem();
        failed.DispatchedAt.ShouldBeNull();
        failed.AttemptCount.ShouldBe(1);
        failed.NextAttemptAt.ShouldBe(fixture.Time.GetUtcNow().AddSeconds(5));
        failed.LastError.ShouldNotBeNull().ShouldContain("Sample listener failure.");
        (await UsageRecordsAsync()).Count.ShouldBe(1, "the listener that succeeded is done");

        (await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation)).ShouldBe(
            0,
            "the retry is not due yet"
        );

        fixture.Time.Advance(TimeSpan.FromSeconds(5));
        await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        (await OutboxAsync()).ShouldHaveSingleItem().DispatchedAt.ShouldNotBeNull();
        (await UsageRecordsAsync()).Count.ShouldBe(1, "the inbox kept the successful listener from running again");
        Probe.Calls.ShouldBe(2);
    }

    [Fact]
    public async Task ProcessBatch_AfterEightFailedAttempts_MarksTheEventFailed()
    {
        await UseNewItemAsync();
        Probe.FailuresRemaining = int.MaxValue;

        for (int attempt = 1; attempt <= OutboxProcessor.MaxAttempts; attempt++)
        {
            (await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation)).ShouldBe(1);
            fixture.Time.Advance(TimeSpan.FromHours(1));
        }

        OutboxMessage message = (await OutboxAsync()).ShouldHaveSingleItem();
        message.AttemptCount.ShouldBe(OutboxProcessor.MaxAttempts);
        message.FailedAt.ShouldNotBeNull();
        (await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation)).ShouldBe(0);
    }

    [Fact]
    public async Task ProcessBatch_ForAListenersChanges_RecordsTheSystemUser()
    {
        await UseNewItemAsync();

        await Processor.ProcessBatchAsync(SampleModuleDefinition.ModuleName, Cancellation);

        (await UsageRecordsAsync()).ShouldHaveSingleItem().CreatedBy.ShouldBe(SystemUser.Id);
    }

    private async Task<SampleItemId> UseNewItemAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var item = SampleItem.Create("Stage", 1m);
        item.Use();
        context.SampleItems.Add(item);
        await context.SaveChangesAsync(Cancellation);
        return item.Id;
    }

    private Task<List<SampleUsageRecord>> UsageRecordsAsync() => ReadAsync(context => context.Set<SampleUsageRecord>());

    private Task<List<OutboxMessage>> OutboxAsync() => ReadAsync(context => context.Set<OutboxMessage>());

    private Task<List<InboxMessage>> InboxAsync() => ReadAsync(context => context.Set<InboxMessage>());

    private async Task<List<T>> ReadAsync<T>(Func<SampleDbContext, IQueryable<T>> query)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<SampleDbContext>()).ToListAsync(Cancellation);
    }
}
