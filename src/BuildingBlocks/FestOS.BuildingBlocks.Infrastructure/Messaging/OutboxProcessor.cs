using System.Diagnostics;
using System.Text.Json;
using FestOS.BuildingBlocks.Contracts;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Delivers a module's waiting events (building-blocks §7). A message that every listener handled is
/// marked dispatched; one that a listener failed waits for a retry at growing intervals, and after
/// <see cref="MaxAttempts"/> attempts is marked failed until someone retries it (building-blocks BB-05).
/// </summary>
public sealed partial class OutboxProcessor(
    IServiceScopeFactory scopes,
    IEventBus bus,
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

                // Unqualified: the module role's search_path starts with the module's schema.
                List<OutboxMessage> messages = await context
                    .Set<OutboxMessage>()
                    .FromSql(
                        $"""
                        SELECT * FROM outbox_messages
                        WHERE dispatched_at IS NULL AND failed_at IS NULL
                          AND (next_attempt_at IS NULL OR next_attempt_at <= {now})
                        ORDER BY sequence
                        LIMIT {BatchSize}
                        FOR UPDATE SKIP LOCKED
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
                LogFailed(logger, exception, message.Id, moduleName, message.AttemptCount);
            }
            else
            {
                message.NextAttemptAt = now + RetryDelays[Math.Min(message.AttemptCount, RetryDelays.Length) - 1];
                LogRetrying(logger, exception, message.Id, moduleName, message.AttemptCount);
            }
        }
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
