using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>
/// Idempotency keys in the unit of work (api §10): the key is stored first, the result last, in the
/// command's transaction. The HTTP filter sets the key in the request's scope; these tests set it directly.
/// </summary>
public sealed class IdempotencyTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static readonly string Fingerprint = new('a', 64);
    private static readonly string OtherFingerprint = new('b', 64);

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task HandleAsync_WithANewKey_RunsTheCommandAndStoresItsResult()
    {
        var key = Guid.CreateVersion7();

        (SampleItemId id, bool replayed) = await SendAsync<CreateSampleItemCommand, SampleItemId>(
            new("Stage", 10m),
            key
        );

        replayed.ShouldBeFalse();
        IdempotencyKey stored = (await LoadKeysAsync()).ShouldHaveSingleItem();
        stored.Key.ShouldBe(key);
        stored.UserId.ShouldBe(fixture.CurrentUser.UserId);
        stored.Fingerprint.ShouldBe(Fingerprint);
        stored.Result.ShouldBe($"\"{id.Value}\"");
        stored.CreatedAt.ShouldBe(fixture.Time.GetUtcNow());
    }

    [Fact]
    public async Task HandleAsync_RetriedWithTheSameKey_ReturnsTheStoredResultWithoutRunningAgain()
    {
        var key = Guid.CreateVersion7();
        (SampleItemId first, _) = await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m), key);

        (SampleItemId retried, bool replayed) = await SendAsync<CreateSampleItemCommand, SampleItemId>(
            new("Stage", 10m),
            key
        );

        replayed.ShouldBeTrue();
        retried.ShouldBe(first);
        (await CountItemsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_WithTheSameKeyForADifferentRequest_IsRejected()
    {
        var key = Guid.CreateVersion7();
        await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m), key);

        await Should.ThrowAsync<IdempotencyKeyReusedException>(() =>
            SendAsync<CreateSampleItemCommand, SampleItemId>(new("Truss", 5m), key, OtherFingerprint)
        );

        (await CountItemsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCommandFails_KeepsNoKeySoARetryRunsAgain()
    {
        SampleItemId id = (await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m))).Result;
        var key = Guid.CreateVersion7();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            SendAsync<AddSamplePartCommand, SampleItemPartId>(new(id, "Leg", FailAfterSaving: true), key)
        );

        (await LoadKeysAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithAnotherUsersKey_RunsAsANewRequest()
    {
        var key = Guid.CreateVersion7();
        await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m), key);
        fixture.CurrentUser.UserId = Guid.CreateVersion7();

        (_, bool replayed) = await SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage copy", 10m), key);

        replayed.ShouldBeFalse();
        (await CountItemsAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task HandleAsync_ForTheSecondCommandOfARequest_DoesNotUseTheKeyAgain()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        IdempotencyRequest idempotency = scope.ServiceProvider.GetRequiredService<IdempotencyRequest>();
        idempotency.Set(Guid.CreateVersion7(), Fingerprint);

        SampleItemId id = await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<CreateSampleItemCommand, SampleItemId>>()
            .HandleAsync(new("Stage", 10m), Cancellation);
        await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<AddSamplePartCommand, SampleItemPartId>>()
            .HandleAsync(new(id, "Leg"), Cancellation);

        idempotency.Replayed.ShouldBeFalse();
        (await LoadKeysAsync()).ShouldHaveSingleItem().Result.ShouldBe($"\"{id.Value}\"");
    }

    [Fact]
    public async Task HandleAsync_WhileAnEarlierRequestWithTheKeyRuns_WaitsAndReturnsItsResult()
    {
        var key = Guid.CreateVersion7();
        var earlierResult = new SampleItemId(Guid.CreateVersion7());
        await using AsyncServiceScope earlier = fixture.Services.CreateAsyncScope();
        await using IDbContextTransaction running = await StoreKeyWithoutCommittingAsync(earlier, key, earlierResult);

        Task<(SampleItemId Result, bool Replayed)> retry = SendAsync<CreateSampleItemCommand, SampleItemId>(
            new("Stage", 10m),
            key
        );
        await Task.Delay(TimeSpan.FromMilliseconds(300), Cancellation);
        retry.IsCompleted.ShouldBeFalse("the retry waits for the earlier request's transaction");
        await running.CommitAsync(Cancellation);

        (SampleItemId result, bool replayed) = await retry;
        replayed.ShouldBeTrue();
        result.ShouldBe(earlierResult);
        (await CountItemsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_WhenTheEarlierRequestOutlastsTheLockTimeout_ReportsItStillRuns()
    {
        var key = Guid.CreateVersion7();
        await using AsyncServiceScope earlier = fixture.Services.CreateAsyncScope();
        await using IDbContextTransaction running = await StoreKeyWithoutCommittingAsync(
            earlier,
            key,
            new SampleItemId(Guid.CreateVersion7())
        );

        // A short lock timeout for this host's sessions instead of the module role's 10 seconds.
        using IHost impatient = fixture.CreateHost(
            fixture.Time,
            settings: new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:festos"] = fixture.ConnectionString + ";Options=-c lock_timeout=300",
            }
        );

        await Should.ThrowAsync<IdempotencyKeyInProgressException>(() =>
            SendAsync<CreateSampleItemCommand, SampleItemId>(new("Stage", 10m), key, services: impatient.Services)
        );
    }

    private async Task<(TResult Result, bool Replayed)> SendAsync<TCommand, TResult>(
        TCommand command,
        Guid? key = null,
        string? fingerprint = null,
        IServiceProvider? services = null
    )
        where TCommand : ICommand<TResult>
    {
        await using AsyncServiceScope scope = (services ?? fixture.Services).CreateAsyncScope();
        IdempotencyRequest idempotency = scope.ServiceProvider.GetRequiredService<IdempotencyRequest>();
        if (key is { } value)
        {
            // What the Idempotency-Key filter does for an HTTP request.
            idempotency.Set(value, fingerprint ?? Fingerprint);
        }

        TResult result = await scope
            .ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>()
            .HandleAsync(command, Cancellation);
        return (result, idempotency.Replayed);
    }

    // An earlier request that stored its key and result but has not committed yet.
    private async Task<IDbContextTransaction> StoreKeyWithoutCommittingAsync(
        AsyncServiceScope scope,
        Guid key,
        SampleItemId result
    )
    {
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Cancellation);
        string json = $"\"{result.Value}\"";
        await context.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO idempotency_keys (user_id, key, fingerprint, result, created_at)
            VALUES ({fixture.CurrentUser.UserId}, {key}, {Fingerprint}, {json}::jsonb, {fixture.Time.GetUtcNow()})
            """,
            Cancellation
        );
        return transaction;
    }

    private async Task<List<IdempotencyKey>> LoadKeysAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<SampleDbContext>()
            .Set<IdempotencyKey>()
            .ToListAsync(Cancellation);
    }

    private async Task<int> CountItemsAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<SampleDbContext>().SampleItems.CountAsync(Cancellation);
    }
}
