using System.Diagnostics.Metrics;
using FestOS.BuildingBlocks.Infrastructure.Locking;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// Runs every registered <see cref="IScheduledJob"/> on its schedule (ADR-0013, building-blocks §8).
/// Each run takes the job's advisory lock first, so with several application instances only one runs
/// it; the others skip that run.
/// </summary>
public sealed partial class ScheduledJobRunner(
    IEnumerable<IScheduledJob> jobs,
    ModuleCatalog modules,
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    TimeProvider timeProvider,
    IMeterFactory meterFactory,
    ILogger<ScheduledJobRunner> logger
) : BackgroundService
{
    private readonly Histogram<double> _duration = meterFactory
        .Create(MessagingMetrics.MeterName)
        .CreateHistogram<double>("festos.jobs.run.duration", unit: "s", description: "Duration of scheduled job runs.");

    /// <summary>
    /// Runs the job once if its lock is free and returns whether it ran. Failures are logged and
    /// measured, not thrown, so one failing run does not stop the schedule.
    /// </summary>
    public async Task<bool> RunOnceAsync(IScheduledJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);
        string schema = modules
            .Modules.Single(module => string.Equals(module.Name, job.ModuleName, StringComparison.Ordinal))
            .Schema;
        string lockKey = $"{schema}:job:{job.Name}";

        // A session lock lives on its own connection outside the pool (database §11.3); closing it
        // releases the lock even if the explicit release fails.
        var connectionString = new NpgsqlConnectionStringBuilder(
            DatabaseConnections.ForRole(configuration, DatabaseRoles.ForModule(schema))
        )
        {
            Pooling = false,
            ApplicationName = $"festos-{schema}-jobs",
        };
        await using var connection = new NpgsqlConnection(connectionString.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        if (!await AdvisoryLocks.TryAcquireSessionLockAsync(connection, lockKey, cancellationToken))
        {
            LogSkipped(logger, job.Name, job.ModuleName);
            return false;
        }

        long startedAt = timeProvider.GetTimestamp();
        LogStarted(logger, job.Name, job.ModuleName);
        try
        {
            await using AsyncServiceScope scope = scopes.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<ActingUser>().ActAsSystem();
            await job.RunAsync(scope.ServiceProvider, cancellationToken);

            TimeSpan elapsed = timeProvider.GetElapsedTime(startedAt);
            Record(job, elapsed, "succeeded");
            LogFinished(logger, job.Name, job.ModuleName, elapsed.TotalMilliseconds);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Record(job, timeProvider.GetElapsedTime(startedAt), "failed");
            LogFailed(logger, exception, job.Name, job.ModuleName);
        }
        finally
        {
            await AdvisoryLocks.ReleaseSessionLockAsync(connection, lockKey, CancellationToken.None);
        }

        return true;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (configuration.GetConnectionString(DatabaseConnections.ConnectionStringName) is null)
        {
            LogDisabled(logger);
            return Task.CompletedTask;
        }

        return Task.WhenAll(jobs.Select(job => RunOnScheduleAsync(job, stoppingToken)));
    }

    private async Task RunOnScheduleAsync(IScheduledJob job, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            TimeSpan wait = job.Schedule.NextAfter(now) - now;
            try
            {
                await Task.Delay(wait > TimeSpan.Zero ? wait : TimeSpan.Zero, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // The host is stopping; waiting for the next run is not a failure.
                return;
            }

            try
            {
                await RunOnceAsync(job, stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // The lock connection could not be opened; the next run tries again.
                LogFailed(logger, exception, job.Name, job.ModuleName);
            }
        }
    }

    private void Record(IScheduledJob job, TimeSpan elapsed, string outcome) =>
        _duration.Record(elapsed.TotalSeconds, new("festos.job.name", job.Name), new("festos.job.outcome", outcome));

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Scheduled jobs are off: the database connection string is not configured"
    )]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobName} of module {ModuleName} started")]
    private static partial void LogStarted(ILogger logger, string jobName, string moduleName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Job {JobName} of module {ModuleName} finished in {ElapsedMilliseconds} ms"
    )]
    private static partial void LogFinished(
        ILogger logger,
        string jobName,
        string moduleName,
        double elapsedMilliseconds
    );

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Job {JobName} of module {ModuleName} skipped: another instance is running it"
    )]
    private static partial void LogSkipped(ILogger logger, string jobName, string moduleName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Job {JobName} of module {ModuleName} failed")]
    private static partial void LogFailed(ILogger logger, Exception exception, string jobName, string moduleName);
}
