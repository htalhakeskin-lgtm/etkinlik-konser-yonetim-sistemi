using FestOS.BuildingBlocks.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

/// <summary>Deletes sign-in attempts older than 90 days, since they hold personal data (security §10).</summary>
internal sealed class LoginAttemptCleanupJob(TimeProvider timeProvider) : IScheduledJob
{
    /// <summary>How long an attempt is kept.</summary>
    public static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(90);

    private const int BatchSize = 1000;

    public string Name => "login-attempt-cleanup";

    public string ModuleName => IdentityModuleDefinition.ModuleName;

    // Nightly, in Istanbul time, after the platform's cleanups.
    public JobSchedule Schedule { get; } = JobSchedule.Cron("15 5 * * *");

    public async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        IdentityDbContext context = services.GetRequiredService<IdentityDbContext>();
        DateTimeOffset cutoff = timeProvider.GetUtcNow() - RetentionPeriod;

        // A technical table of the Infrastructure layer, without history, so set-based deletes are allowed (database §14.2).
        int deleted;
        do
        {
            deleted = await context
                .Set<LoginAttempt>()
                .Where(attempt => attempt.OccurredAt < cutoff)
                .OrderBy(attempt => attempt.OccurredAt)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);
        } while (deleted == BatchSize);
    }
}
