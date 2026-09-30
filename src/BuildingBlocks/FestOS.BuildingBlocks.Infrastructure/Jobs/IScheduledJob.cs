namespace FestOS.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// A short, repeatable job that runs on a schedule (ADR-0013), named <c>{Name}Job</c> and owned by a
/// module. Module jobs only trigger an Application command; the work lives in the command. A job must
/// give the same result when it runs twice.
/// </summary>
public interface IScheduledJob
{
    /// <summary>The job's name, e.g. <c>automatic-transitions</c>; part of its lock key and metric tag.</summary>
    string Name { get; }

    /// <summary>The module the job belongs to; its lock is taken on that module's connection.</summary>
    string ModuleName { get; }

    /// <summary>When the job runs.</summary>
    JobSchedule Schedule { get; }

    /// <summary>Runs once, in its own scope, acting as the system user.</summary>
    Task RunAsync(IServiceProvider services, CancellationToken cancellationToken);
}
