using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>Commands through the full decorator chain, ending in the unit of work (building-blocks §4).</summary>
public sealed class UnitOfWorkTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task HandleAsync_ForACommand_SavesWhatTheHandlerChanged()
    {
        SampleItemId id = await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m));

        SampleItem stored = await LoadAsync(id);
        stored.Name.ShouldBe("Stage");
        stored.Version.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheHandlerFailsAfterWriting_RollsBackTheTransaction()
    {
        SampleItemId id = await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            SendAsync<AddSamplePartCommand, SampleItemPartId>(new(id, "Leg", FailAfterSaving: true))
        );

        SampleItem stored = await LoadAsync(id);
        stored.Parts.ShouldBeEmpty();
        stored.Version.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_ForSeveralCommandsInOneScope_SavesEachInItsOwnTransaction()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        ICommandHandler<CreateSampleItemCommand, SampleItemId> create = scope.ServiceProvider.GetRequiredService<
            ICommandHandler<CreateSampleItemCommand, SampleItemId>
        >();
        ICommandHandler<AddSamplePartCommand, SampleItemPartId> addPart = scope.ServiceProvider.GetRequiredService<
            ICommandHandler<AddSamplePartCommand, SampleItemPartId>
        >();

        SampleItemId id = await create.HandleAsync(new("Stage", 10m), Cancellation);
        await addPart.HandleAsync(new(id, "Leg"), Cancellation);

        SampleItem stored = await LoadAsync(id);
        stored.Parts.ShouldHaveSingleItem().Label.ShouldBe("Leg");
        stored.Version.ShouldBe(2);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-011")]
    public async Task HandleAsync_BasedOnTheCurrentVersion_SavesTheChange()
    {
        SampleItemId id = await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m));

        await SendAsync<UseSampleItemCommand, bool>(new(id), expectedVersion: 1);

        SampleItem stored = await LoadAsync(id);
        stored.Status.ShouldBe(SampleItemStatus.InUse);
        stored.Version.ShouldBe(2);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-011")]
    public async Task HandleAsync_BasedOnAnOldVersion_RejectsTheChangeAndSavesNothing()
    {
        SampleItemId id = await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m));
        await SendAsync<AddSamplePartCommand, SampleItemPartId>(new(id, "Leg"));

        await Should.ThrowAsync<ConcurrencyConflictException>(() =>
            SendAsync<UseSampleItemCommand, bool>(new(id), expectedVersion: 1)
        );

        SampleItem stored = await LoadAsync(id);
        stored.Status.ShouldBe(SampleItemStatus.Draft);
        stored.Version.ShouldBe(2);
    }

    private async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, int? expectedVersion = null)
        where TCommand : ICommand<TResult>
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        if (expectedVersion is { } version)
        {
            // What the If-Match filter does for an HTTP request.
            scope.ServiceProvider.GetRequiredService<ExpectedVersion>().Set(version);
        }

        return await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>()
            .HandleAsync(command, Cancellation);
    }

    private async Task<SampleItem> LoadAsync(SampleItemId id)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<SampleDbContext>()
            .SampleItems.Include(sample => sample.Parts)
            .SingleAsync(sample => sample.Id == id, Cancellation);
    }
}
