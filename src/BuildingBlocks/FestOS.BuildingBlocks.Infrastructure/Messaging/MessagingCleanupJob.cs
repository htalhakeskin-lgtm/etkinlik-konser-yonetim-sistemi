using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Deletes a module's delivered outbox messages and inbox records older than the retention period, in
/// small batches (database §15). Failed messages stay until someone handles them. One instance per
/// module, registered by <c>AddModules</c>.
/// </summary>
internal sealed class MessagingCleanupJob(
    string moduleName,
    IOptions<MessagingOptions> options,
    TimeProvider timeProvider
) : IScheduledJob
{
    private const int BatchSize = 1000;

    public string Name => "messaging-cleanup";

    public string ModuleName => moduleName;

    public JobSchedule Schedule { get; } = JobSchedule.Cron(options.Value.CleanupSchedule);

    public async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        ModuleDbContext context = services.GetRequiredKeyedService<ModuleDbContext>(moduleName);
        DateTimeOffset cutoff = timeProvider.GetUtcNow() - options.Value.RetentionPeriod;

        // Technical tables without history, so set-based deletes are allowed here (database §14.2).
        List<Guid> delivered;
        do
        {
            delivered = await context
                .Set<OutboxMessage>()
                .Where(message => message.DispatchedAt != null && message.DispatchedAt < cutoff)
                .OrderBy(message => message.Sequence)
                .Select(message => message.Id)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            await context
                .Set<OutboxMessage>()
                .Where(message => delivered.Contains(message.Id))
                .ExecuteDeleteAsync(cancellationToken);
        } while (delivered.Count == BatchSize);

        List<Guid> processed;
        do
        {
            processed = await context
                .Set<InboxMessage>()
                .Where(message => message.ProcessedAt < cutoff)
                .Select(message => message.MessageId)
                .Distinct()
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            await context
                .Set<InboxMessage>()
                .Where(message => processed.Contains(message.MessageId) && message.ProcessedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);
        } while (processed.Count == BatchSize);
    }
}
