using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.Modules.Identity.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>
/// Deletes the sessions that have ended (BR-SYS-008). An ended session is refused anyway when its cookie
/// comes back; this only keeps the table small.
/// </summary>
internal sealed class SessionCleanupJob(TimeProvider timeProvider, IdentityModuleOptions options) : IScheduledJob
{
    public string Name => "session-cleanup";

    public string ModuleName => IdentityModuleDefinition.ModuleName;

    // Nightly, in Istanbul time, after the platform's cleanups.
    public JobSchedule Schedule { get; } = JobSchedule.Cron("0 5 * * *");

    public async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset idleSince = now - options.SessionIdleTimeout;

        // A technical table of the Infrastructure layer, without history, so set-based deletes are allowed (database §14.2).
        await services
            .GetRequiredService<IdentityDbContext>()
            .Set<Session>()
            .Where(session => session.ExpiresAt <= now || session.LastSeenAt <= idleSince)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
