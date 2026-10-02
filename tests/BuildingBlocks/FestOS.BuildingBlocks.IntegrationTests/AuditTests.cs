using System.Diagnostics;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>The change history written with every save (building-blocks §6, database §14.2).</summary>
[Trait("Rule", "BR-SYS-010")]
public sealed class AuditTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Save_ForANewRecord_RecordsWhoWhenAndEveryValue()
    {
        SampleItem item = await CreateItemAsync("Stage", 12.3456m);

        AuditEntry entry = (await EntriesAsync()).ShouldHaveSingleItem();
        entry.Module.ShouldBe("sample");
        entry.EntityType.ShouldBe(nameof(SampleItem));
        entry.EntityId.ShouldBe(item.Id.Value);
        entry.RootType.ShouldBe(nameof(SampleItem), "a root is its own root");
        entry.RootId.ShouldBe(item.Id.Value);
        entry.Action.ShouldBe(AuditAction.Created);
        entry.ActorId.ShouldBe(fixture.CurrentUser.UserId);
        entry.ActorName.ShouldBe(fixture.CurrentUser.DisplayName);
        entry.OccurredAt.ShouldBe(fixture.Time.GetUtcNow());

        JsonElement changes = Parse(entry);
        changes.GetProperty("name").GetProperty("new").GetString().ShouldBe("Stage");
        changes.GetProperty("unitPrice").GetProperty("new").GetDecimal().ShouldBe(12.3456m);
        changes.GetProperty("status").GetProperty("new").GetString().ShouldBe("draft");
        changes.TryGetProperty("version", out _).ShouldBeFalse("bookkeeping fields stay out of the changes");
    }

    [Fact]
    public async Task Save_ForAChangedField_RecordsOnlyThatFieldWithItsOldAndNewValue()
    {
        SampleItem item = await CreateItemAsync("Stage", 1m);

        await ChangeAsync(item.Id, loaded => loaded.Rename("Main stage", internalCode: "X-1"));

        AuditEntry entry = (await EntriesAsync()).Last();
        entry.Action.ShouldBe(AuditAction.Updated);
        JsonElement changes = Parse(entry);
        changes.GetProperty("name").GetProperty("old").GetString().ShouldBe("Stage");
        changes.GetProperty("name").GetProperty("new").GetString().ShouldBe("Main stage");
        changes.EnumerateObject().Select(field => field.Name).ShouldBe(["name"], "internalCode is not audited");
    }

    [Fact]
    public async Task Save_WhenTheStatusChanges_RecordsAStatusChangeAndTheHandlersChange()
    {
        SampleItem item = await CreateItemAsync("Stage", 1m);

        await ChangeAsync(item.Id, loaded => loaded.Use());

        List<AuditEntry> entries = [.. (await EntriesAsync()).Skip(1)];
        AuditEntry statusChange = entries.Single(entry =>
            string.Equals(entry.EntityType, nameof(SampleItem), StringComparison.Ordinal)
        );
        statusChange.Action.ShouldBe(AuditAction.StatusChanged);
        Parse(statusChange).GetProperty("status").GetProperty("new").GetString().ShouldBe("inUse");
        entries
            .Single(entry => string.Equals(entry.EntityType, nameof(SampleItemPart), StringComparison.Ordinal))
            .Action.ShouldBe(AuditAction.Created);
    }

    [Fact]
    public async Task Save_WhenOnlyAChildChanges_RecordsTheChildButNotTheRootsVersionBump()
    {
        SampleItem item = await CreateItemAsync("Stage", 1m);

        await ChangeAsync(item.Id, loaded => loaded.AddPart("Leg"));

        AuditEntry entry = (await EntriesAsync()).Skip(1).ShouldHaveSingleItem();
        entry.EntityType.ShouldBe(nameof(SampleItemPart));
        Parse(entry).GetProperty("sampleItemId").GetProperty("new").GetGuid().ShouldBe(item.Id.Value);
    }

    [Fact]
    public async Task Save_ForAChildAddedOrRemoved_RecordsTheRootItBelongsTo()
    {
        SampleItem item = await CreateItemAsync("Stage", 1m);
        await ChangeAsync(item.Id, loaded => loaded.AddPart("Leg"));
        fixture.Time.Advance(TimeSpan.FromMinutes(1));

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            SampleItem loaded = await context
                .SampleItems.Include(sample => sample.Parts)
                .SingleAsync(sample => sample.Id == item.Id, Cancellation);
            context.Remove(loaded.Parts.Single());
            await context.SaveChangesAsync(Cancellation);
        }

        List<AuditEntry> partEntries =
        [
            .. (await EntriesAsync()).Where(entry =>
                string.Equals(entry.EntityType, nameof(SampleItemPart), StringComparison.Ordinal)
            ),
        ];
        partEntries.Select(entry => entry.Action).ShouldBe([AuditAction.Created, AuditAction.Deleted]);
        partEntries.ShouldAllBe(entry => entry.RootType == nameof(SampleItem) && entry.RootId == item.Id.Value);
    }

    [Fact]
    public async Task Save_ForADeletedRecord_RecordsItsOldValues()
    {
        SampleItem item = await CreateItemAsync("Stage", 1m);
        fixture.Time.Advance(TimeSpan.FromMinutes(1));

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            context.SampleItems.Remove(
                await context.SampleItems.SingleAsync(sample => sample.Id == item.Id, Cancellation)
            );
            await context.SaveChangesAsync(Cancellation);
        }

        AuditEntry entry = (await EntriesAsync()).Last();
        entry.Action.ShouldBe(AuditAction.Deleted);
        Parse(entry).GetProperty("name").GetProperty("old").GetString().ShouldBe("Stage");
    }

    [Fact]
    public async Task Save_InsideATrace_RecordsTheTraceId()
    {
        using Activity activity = new Activity("audit-test").Start();

        await CreateItemAsync("Stage", 1m);

        (await EntriesAsync()).ShouldHaveSingleItem().TraceId.ShouldBe(activity.TraceId.ToHexString());
    }

    [Fact]
    public async Task Command_WhenItRollsBack_LeavesNoHistory()
    {
        SampleItem item = await CreateItemAsync("Stage", 1m);

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            ICommandHandler<AddSamplePartCommand, SampleItemPartId> handler = scope.ServiceProvider.GetRequiredService<
                ICommandHandler<AddSamplePartCommand, SampleItemPartId>
            >();
            await Should.ThrowAsync<InvalidOperationException>(() =>
                handler.HandleAsync(new(item.Id, "Leg", FailAfterSaving: true), Cancellation)
            );
        }

        (await EntriesAsync()).ShouldHaveSingleItem().Action.ShouldBe(AuditAction.Created);
    }

    private static JsonElement Parse(AuditEntry entry) => JsonDocument.Parse(entry.Changes).RootElement;

    private async Task<SampleItem> CreateItemAsync(string name, decimal unitPrice)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var item = SampleItem.Create(name, unitPrice);
        context.SampleItems.Add(item);
        await context.SaveChangesAsync(Cancellation);
        return item;
    }

    private async Task ChangeAsync(SampleItemId id, Action<SampleItem> change)
    {
        fixture.Time.Advance(TimeSpan.FromMinutes(1));
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        change(
            await context
                .SampleItems.Include(sample => sample.Parts)
                .SingleAsync(sample => sample.Id == id, Cancellation)
        );
        await context.SaveChangesAsync(Cancellation);
    }

    // Read with the Audit module's own role, the only one allowed to read the history.
    private async Task<List<AuditEntry>> EntriesAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<AuditDbContext>()
            .AuditEntries.OrderBy(entry => entry.OccurredAt)
            .ThenBy(entry => entry.Id)
            .ToListAsync(Cancellation);
    }
}
