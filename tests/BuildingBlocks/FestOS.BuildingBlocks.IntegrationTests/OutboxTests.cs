using System.Diagnostics;
using System.Text.Json;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Modules.Sample.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>Integration events written to the outbox with the change that raised them (building-blocks §7, 05 §9.2).</summary>
public sealed class OutboxTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Save_WhenAHandlerAddsAnIntegrationEvent_WritesItToTheOutboxAsPending()
    {
        SampleItem item = await CreateItemAsync("Stage");

        await ChangeAsync(item.Id, loaded => loaded.Use());

        OutboxMessage message = (await OutboxAsync()).ShouldHaveSingleItem();
        message.Type.ShouldBe(
            $"{typeof(SampleItemUsedIntegrationEvent).FullName}, {typeof(SampleItemUsedIntegrationEvent).Assembly.GetName().Name}"
        );
        message.OrderingKey.ShouldBe(item.Id.Value);
        message.OccurredAt.ShouldBe(fixture.Time.GetUtcNow());
        message.DispatchedAt.ShouldBeNull();
        message.AttemptCount.ShouldBe(0);

        JsonElement payload = JsonDocument.Parse(message.Payload).RootElement;
        payload.GetProperty("sampleItemId").GetGuid().ShouldBe(item.Id.Value);
        payload.GetProperty("messageId").GetGuid().ShouldBe(message.Id);
    }

    [Fact]
    public async Task Save_InsideATrace_KeepsTheTraceContextForTheListeners()
    {
        SampleItem item = await CreateItemAsync("Stage");
        using Activity activity = new Activity("outbox-test").SetIdFormat(ActivityIdFormat.W3C).Start();

        await ChangeAsync(item.Id, loaded => loaded.Use());

        (await OutboxAsync()).ShouldHaveSingleItem().TraceParent.ShouldBe(activity.Id);
    }

    [Fact]
    public async Task Save_WhenTheChangeFails_WritesNoEvent()
    {
        await CreateItemAsync("Stage");
        SampleItem other = await CreateItemAsync("Other stage");

        // Using the item raises the event; taking a name in use breaks a constraint in the same save.
        await Should.ThrowAsync<BusinessRuleViolationException>(() =>
            ChangeAsync(
                other.Id,
                loaded =>
                {
                    loaded.Use();
                    loaded.Rename("Stage");
                }
            )
        );

        (await OutboxAsync()).ShouldBeEmpty();
    }

    private async Task<SampleItem> CreateItemAsync(string name)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var item = SampleItem.Create(name, 1m);
        context.SampleItems.Add(item);
        await context.SaveChangesAsync(Cancellation);
        return item;
    }

    private async Task ChangeAsync(SampleItemId id, Action<SampleItem> change)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        change(
            await context
                .SampleItems.Include(sample => sample.Parts)
                .SingleAsync(sample => sample.Id == id, Cancellation)
        );
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
}
