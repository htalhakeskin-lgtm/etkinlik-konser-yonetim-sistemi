using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>A sample module job that writes a usage record each time it runs.</summary>
internal sealed class RecordingJob : IScheduledJob
{
    public const string JobName = "recording";

    public string Name => JobName;

    public string ModuleName => SampleModuleDefinition.ModuleName;

    public JobSchedule Schedule { get; } = JobSchedule.Every(TimeSpan.FromMinutes(1));

    public async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        SampleDbContext context = services.GetRequiredService<SampleDbContext>();
        context.Add(SampleUsageRecord.Create(Guid.CreateVersion7()));
        await context.SaveChangesAsync(cancellationToken);
    }
}
