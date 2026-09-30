using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>The save steps of <c>ModuleDbContext</c> on a real database (building-blocks §4).</summary>
public sealed class SaveChangesTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SaveChangesAsync_ForANewAggregate_StartsAtVersionOneWithCreationStamps()
    {
        SampleItem item = await CreateItemAsync("Stage");

        SampleItem stored = await LoadAsync(item.Id);
        stored.Version.ShouldBe(1);
        stored.CreatedAt.ShouldBe(fixture.Time.GetUtcNow());
        stored.CreatedBy.ShouldBe(fixture.CurrentUser.UserId);
        stored.UpdatedAt.ShouldBe(stored.CreatedAt);
        stored.UpdatedBy.ShouldBe(stored.CreatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenOnlyAChildChanges_BumpsTheRootVersionAndUpdateStamps()
    {
        SampleItem item = await CreateItemAsync("Stage");
        DateTimeOffset createdAt = fixture.Time.GetUtcNow();
        fixture.Time.Advance(TimeSpan.FromMinutes(5));
        var editor = Guid.CreateVersion7();
        fixture.CurrentUser.UserId = editor;

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            SampleItem loaded = await context
                .SampleItems.Include(sample => sample.Parts)
                .SingleAsync(sample => sample.Id == item.Id, Cancellation);
            loaded.AddPart("Leg");
            await context.SaveChangesAsync(Cancellation);
        }

        SampleItem stored = await LoadAsync(item.Id);
        stored.Version.ShouldBe(2);
        stored.CreatedAt.ShouldBe(createdAt);
        stored.UpdatedAt.ShouldBe(fixture.Time.GetUtcNow());
        stored.UpdatedBy.ShouldBe(editor);
        stored.Parts.ShouldHaveSingleItem().Label.ShouldBe("Leg");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenTheAggregateChangedMeanwhile_ThrowsAConcurrencyConflict()
    {
        SampleItem item = await CreateItemAsync("Stage");
        await using AsyncServiceScope first = fixture.Services.CreateAsyncScope();
        await using AsyncServiceScope second = fixture.Services.CreateAsyncScope();
        SampleDbContext firstContext = first.ServiceProvider.GetRequiredService<SampleDbContext>();
        SampleDbContext secondContext = second.ServiceProvider.GetRequiredService<SampleDbContext>();
        SampleItem firstCopy = await LoadWithPartsAsync(firstContext, item.Id);
        SampleItem secondCopy = await LoadWithPartsAsync(secondContext, item.Id);

        firstCopy.AddPart("Leg");
        await firstContext.SaveChangesAsync(Cancellation);
        secondCopy.AddPart("Arm");

        await Should.ThrowAsync<ConcurrencyConflictException>(() => secondContext.SaveChangesAsync(Cancellation));
    }

    [Fact]
    public async Task SaveChangesAsync_ForARaisedDomainEvent_RunsItsHandlerInTheSameSave()
    {
        SampleItem item = await CreateItemAsync("Stage");

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            SampleItem loaded = await LoadWithPartsAsync(context, item.Id);
            loaded.Use();
            await context.SaveChangesAsync(Cancellation);
        }

        SampleItem stored = await LoadAsync(item.Id);
        stored.Status.ShouldBe(SampleItemStatus.InUse);
        stored.Parts.ShouldHaveSingleItem().Label.ShouldBe("usage");
        stored.Version.ShouldBe(2, "the handler's change is part of the same save");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenHandlersKeepRaisingEvents_StopsAfterFiveRoundsWithoutSaving()
    {
        SampleItem item = await CreateItemAsync("Stage");

        await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            SampleItem loaded = await LoadWithPartsAsync(context, item.Id);
            loaded.Echo();
            loaded.AddPart("Leg");

            await Should.ThrowAsync<InvalidOperationException>(() => context.SaveChangesAsync(Cancellation));
        }

        (await LoadAsync(item.Id)).Parts.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAMappedConstraintIsViolated_ThrowsItsRule()
    {
        await CreateItemAsync("Stage");

        BusinessRuleViolationException exception = await Should.ThrowAsync<BusinessRuleViolationException>(() =>
            CreateItemAsync("Stage")
        );

        exception.RuleCode.ShouldBe(SampleRuleCodes.NameIsUnique);
        exception.InnerException.ShouldBeOfType<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_Synchronously_Throws()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        context.SampleItems.Add(SampleItem.Create("Stage", 1m));

        Should.Throw<InvalidOperationException>(() => context.SaveChanges());
    }

    private async Task<SampleItem> CreateItemAsync(string name)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var item = SampleItem.Create(name, 10m);
        context.SampleItems.Add(item);
        await context.SaveChangesAsync(Cancellation);
        return item;
    }

    private async Task<SampleItem> LoadAsync(SampleItemId id)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await LoadWithPartsAsync(scope.ServiceProvider.GetRequiredService<SampleDbContext>(), id);
    }

    private static Task<SampleItem> LoadWithPartsAsync(SampleDbContext context, SampleItemId id) =>
        context.SampleItems.Include(sample => sample.Parts).SingleAsync(sample => sample.Id == id, Cancellation);
}
