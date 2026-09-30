using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Jobs;

/// <summary>Registers scheduled jobs; a module calls this from its <c>RegisterServices</c>.</summary>
public static class ScheduledJobExtensions
{
    /// <summary>Registers the job; the scheduled job runner starts it with the host.</summary>
    public static IServiceCollection AddScheduledJob<TJob>(this IServiceCollection services)
        where TJob : class, IScheduledJob => services.AddSingleton<IScheduledJob, TJob>();
}
