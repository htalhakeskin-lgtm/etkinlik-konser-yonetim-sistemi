using System.Diagnostics;
using System.Text.Json;
using FestOS.BuildingBlocks.Contracts;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Delivers a module's waiting events (building-blocks §7). A message that every listener handled is
/// marked dispatched; one that a listener failed waits for a retry at growing intervals, and after
/// <see cref="MaxAttempts"/> attempts is marked failed until someone retries it (building-blocks BB-05).
/// </summary>
public sealed partial class OutboxProcessor(
    IServiceScopeFactory scopes,
    IEventBus bus,
    MessagingMetrics metrics,
    IOptions<MessagingOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger
)
{
    /// <summary>How many messages one batch takes.</summary>
    public const int BatchSize = 20;

    /// <summary>Failed deliveries before a message is marked failed.</summary>
    public const int MaxAttempts = 8;

    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromHours(1),
    ];

    /// <summary>
    /// Delivers one batch of the module's due messages, in order, and returns how many it took. The rows
    /// are locked with <c>FOR UPDATE SKIP LOCKED</c>, so another instance would take different ones.
    /// </summary>
    public async Task<int> ProcessBatchAsync(string moduleName, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ActingUser>().ActAsSystem();
        ModuleDbContext context = scope.ServiceProvider.GetRequiredKeyedService<ModuleDbContext>(moduleName);

        return await ModuleUnitOfWork.RunAsync(
            context,
            scope.ServiceProvider.GetRequiredService<Outbox>(),
            async retryCancellationToken =>
            {
                DateTimeOffset now = timeProvider.GetUtcNow();

                // Unqualified: the module role's search_path starts with the module's schema. A message
                // waits while an older one with the same ordering key is undelivered, even a failed one,
                // so a batch holds at most one message per key and each key's events stay in order.
                List<OutboxMessage> messages = await context
                    .Set<OutboxMessage>()
                    .FromSql(
                        $"""
                        SELECT message.* FROM outbox_messages message
                        WHERE message.dispatched_at IS NULL AND message.failed_at IS NULL
                          AND (message.next_attempt_at IS NULL OR message.next_attempt_at <= {now})
                          AND NOT EXISTS (
                              SELECT 1 FROM outbox_messages older
                              WHERE older.ordering_key = message.ordering_key
                                AND older.sequence < message.sequence
                                AND older.dispatched_at IS NULL)
                        ORDER BY message.sequence
                        LIMIT {BatchSize}
                        FOR UPDATE OF message SKIP LOCKED
                        """
                    )
                    .ToListAsync(retryCancellationToken);

                foreach (OutboxMessage message in messages)
                {
                    await DeliverAsync(moduleName, message, retryCancellationToken);
                }

                return messages.Count;
            },
            cancellationToken
        );
    }

    private async Task DeliverAsync(string moduleName, OutboxMessage message, CancellationToken cancellationToken)
    {
        string eventName = message.Type.Split(',')[0].Split('.')[^1];
        using Activity? activity = MessagingTelemetry.StartDelivery(message, moduleName, eventName);

        try
        {
            Type type = Type.GetType(message.Type, throwOnError: true)!;
            var integrationEvent = (IIntegrationEvent)
                JsonSerializer.Deserialize(message.Payload, type, Outbox.JsonOptions)!;
            await bus.PublishAsync(integrationEvent, cancellationToken);
            message.DispatchedAt = timeProvider.GetUtcNow();

            TimeSpan latency = message.DispatchedAt.Value - message.OccurredAt;
            metrics.RecordDelivered(moduleName, eventName, latency);
            if (latency > options.Value.EventLatencyTarget)
            {
                LogSlowDelivery(logger, message.Id, moduleName, latency.TotalMilliseconds);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.AddException(exception);
            activity?.SetStatus(ActivityStatusCode.Error);
            DateTimeOffset now = timeProvider.GetUtcNow();
            message.AttemptCount++;
            message.LastError = Describe(exception);

            if (message.AttemptCount >= MaxAttempts)
            {
                message.FailedAt = now;
                metrics.RecordFailed(moduleName, eventName);
                LogFailed(logger, exception, message.Id, moduleName, message.AttemptCount);
            }
            else
            {
                message.NextAttemptAt = now + RetryDelays[Math.Min(message.AttemptCount, RetryDelays.Length) - 1];
                LogRetrying(logger, exception, message.Id, moduleName, message.AttemptCount);
            }
        }
    }

    /// <summary>How many of the module's messages still wait for delivery, failed ones excluded.</summary>
    public async Task<int> CountPendingAsync(string moduleName, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ActingUser>().ActAsSystem();
        int pending = await scope
            .ServiceProvider.GetRequiredKeyedService<ModuleDbContext>(moduleName)
            .Set<OutboxMessage>()
            .CountAsync(message => message.DispatchedAt == null && message.FailedAt == null, cancellationToken);
        metrics.SetPending(moduleName, pending);
        return pending;
    }

    private static string Describe(Exception exception)
    {
        string description = exception is AggregateException aggregate
            ? string.Join(" | ", aggregate.InnerExceptions.Select(inner => $"{inner.GetType().Name}: {inner.Message}"))
            : $"{exception.GetType().Name}: {exception.Message}";
        return description.Length <= MessagingModel.ErrorMaxLength
            ? description
            : description[..MessagingModel.ErrorMaxLength];
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Delivering event {MessageId} of module {ModuleName} failed on attempt {AttemptCount}; it will be retried"
    )]
    private static partial void LogRetrying(
        ILogger logger,
        Exception exception,
        Guid messageId,
        string moduleName,
        int attemptCount
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event {MessageId} of module {ModuleName} reached its listeners after {LatencyMilliseconds} ms, over the target"
    )]
    private static partial void LogSlowDelivery(
        ILogger logger,
        Guid messageId,
        string moduleName,
        double latencyMilliseconds
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Delivering event {MessageId} of module {ModuleName} failed {AttemptCount} times; it is marked failed"
    )]
    private static partial void LogFailed(
        ILogger logger,
        Exception exception,
        Guid messageId,
        string moduleName,
        int attemptCount
    );
}
