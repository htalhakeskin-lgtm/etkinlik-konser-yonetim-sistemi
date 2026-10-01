using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.Modules.Identity.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>
/// Deletes the sessions that have ended (BR-SYS-008). An ended session is refused anyway when its cookie
/// comes back; this only keeps the table small.
/// </summary>
internal sealed class SessionsCleanupJob(TimeProvider timeProvider, IdentityModuleOptions options) : IScheduledJob
{
    public string Name => "sessions-cleanup";

    public string ModuleName => IdentityModuleDefinition.ModuleName;

    // Hourly, so the table holds little more than the open sessions (identity §8).
    public JobSchedule Schedule { get; } = JobSchedule.Every(TimeSpan.FromHours(1));

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
