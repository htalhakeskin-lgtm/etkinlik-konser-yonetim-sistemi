using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Idempotency;

/// <summary>
/// Deletes a module's idempotency keys older than 24 hours, in small batches (api §10). One instance per
/// module, registered by <c>AddModules</c>.
/// </summary>
internal sealed class IdempotencyCleanupJob(string moduleName, TimeProvider timeProvider) : IScheduledJob
{
    /// <summary>How long a retry is answered from its stored result (api §10).</summary>
    public static readonly TimeSpan RetentionPeriod = TimeSpan.FromHours(24);

    private const int BatchSize = 1000;

    public string Name => "idempotency-cleanup";

    public string ModuleName => moduleName;

    // Nightly, in Istanbul time, after the messaging cleanup.
    public JobSchedule Schedule { get; } = JobSchedule.Cron("30 4 * * *");

    public async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        ModuleDbContext context = services.GetRequiredKeyedService<ModuleDbContext>(moduleName);
        DateTimeOffset cutoff = timeProvider.GetUtcNow() - RetentionPeriod;

        // A technical table without history, so set-based deletes are allowed here (database §14.2).
        int deleted;
        do
        {
            deleted = await context
                .Set<IdempotencyKey>()
                .Where(key => key.CreatedAt < cutoff)
                .OrderBy(key => key.CreatedAt)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);
        } while (deleted == BatchSize);
    }
}
