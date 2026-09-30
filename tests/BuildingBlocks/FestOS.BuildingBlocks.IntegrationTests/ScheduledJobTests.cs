using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.BuildingBlocks.Infrastructure.Locking;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Npgsql;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>The scheduled job runner and the messaging cleanup job (ADR-0013, building-blocks §8).</summary>
public sealed class ScheduledJobTests(SampleModuleFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private ScheduledJobRunner Runner => fixture.Services.GetRequiredService<ScheduledJobRunner>();

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RunOnce_ForAJob_RunsItAsTheSystemUser()
    {
        bool ran = await Runner.RunOnceAsync(new RecordingJob(), Cancellation);

        ran.ShouldBeTrue();
        (await UsageRecordsAsync()).ShouldHaveSingleItem().CreatedBy.ShouldBe(SystemUser.Id);
    }

    [Fact]
    public async Task RunOnce_WhileAnotherInstanceRunsTheJob_SkipsThisRun()
    {
        await using var other = new NpgsqlConnection(
            new NpgsqlConnectionStringBuilder(
                DatabaseConnections.ForRole(fixture.Services.GetRequiredService<IConfiguration>(), "festos_sample")
            )
            {
                Pooling = false,
            }.ConnectionString
        );
        await other.OpenAsync(Cancellation);
        const string LockKey = $"sample:job:{RecordingJob.JobName}";
        await AdvisoryLocks.TryAcquireSessionLockAsync(other, LockKey, Cancellation);

        bool ran = await Runner.RunOnceAsync(new RecordingJob(), Cancellation);

        ran.ShouldBeFalse();
        (await UsageRecordsAsync()).ShouldBeEmpty();
        await AdvisoryLocks.ReleaseSessionLockAsync(other, LockKey, Cancellation);
    }

    [Fact]
    public async Task Runner_OnTheJobsSchedule_RunsIt()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero));
        using IHost host = fixture.CreateHost(time, services => services.AddScheduledJob<RecordingJob>());
        await host.StartAsync(Cancellation);

        // Moving the clock at every check fires the interval whenever the runner starts waiting.
        await Eventually.WaitUntilAsync(
            async () =>
            {
                time.Advance(TimeSpan.FromMinutes(1));
                return (await UsageRecordsAsync()).Count > 0;
            },
            TimeSpan.FromSeconds(15),
            "the job's interval passed"
        );
        await host.StopAsync(Cancellation);
    }

    [Fact]
    public async Task CleanupJob_ForAModule_DeletesOnlyOldDeliveredMessagesAndInboxRecords()
    {
        DateTimeOffset now = fixture.Time.GetUtcNow();
        var oldDelivered = Guid.CreateVersion7();
        var recentDelivered = Guid.CreateVersion7();
        var oldFailed = Guid.CreateVersion7();
        await SeedAsync(context =>
        {
            context.Add(Message(oldDelivered, now.AddDays(-40), dispatchedAt: now.AddDays(-40)));
            context.Add(Message(recentDelivered, now.AddDays(-2), dispatchedAt: now.AddDays(-2)));
            context.Add(Message(oldFailed, now.AddDays(-40), dispatchedAt: null, failedAt: now.AddDays(-39)));
            context.Add(
                new InboxMessage
                {
                    MessageId = oldDelivered,
                    Handler = "old",
                    ProcessedAt = now.AddDays(-40),
                }
            );
            context.Add(
                new InboxMessage
                {
                    MessageId = recentDelivered,
                    Handler = "recent",
                    ProcessedAt = now.AddDays(-2),
                }
            );
        });
        IScheduledJob cleanup = fixture
            .Services.GetServices<IScheduledJob>()
            .Single(job => job is { Name: "messaging-cleanup", ModuleName: SampleModuleDefinition.ModuleName });

        (await Runner.RunOnceAsync(cleanup, Cancellation)).ShouldBeTrue();

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext reader = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        (await reader.Set<OutboxMessage>().Select(message => message.Id).ToListAsync(Cancellation)).ShouldBe(
            [recentDelivered, oldFailed],
            ignoreOrder: true
        );
        (await reader.Set<InboxMessage>().Select(message => message.Handler).ToListAsync(Cancellation)).ShouldBe([
            "recent",
        ]);
    }

    private static OutboxMessage Message(
        Guid id,
        DateTimeOffset occurredAt,
        DateTimeOffset? dispatchedAt,
        DateTimeOffset? failedAt = null
    ) =>
        new()
        {
            Id = id,
            Type = "Sample, Sample",
            OrderingKey = id,
            Payload = "{}",
            OccurredAt = occurredAt,
            DispatchedAt = dispatchedAt,
            FailedAt = failedAt,
        };

    private async Task SeedAsync(Action<SampleDbContext> seed)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        seed(context);
        await context.SaveChangesAsync(Cancellation);
    }

    private async Task<List<SampleUsageRecord>> UsageRecordsAsync()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        return await scope
            .ServiceProvider.GetRequiredService<SampleDbContext>()
            .Set<SampleUsageRecord>()
            .ToListAsync(Cancellation);
    }
}
