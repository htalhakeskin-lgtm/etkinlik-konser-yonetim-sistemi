using Cronos;
using FestOS.BuildingBlocks.Domain.Time;

namespace FestOS.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// When a scheduled job runs (ADR-0013): at a fixed interval, or at the times of a cron expression read
/// in Europe/Istanbul time, e.g. <c>0 3 * * *</c> for every day at 03:00 in Istanbul.
/// </summary>
public sealed class JobSchedule
{
    private readonly TimeSpan? _interval;
    private readonly CronExpression? _cron;
    private readonly string _description;

    private JobSchedule(TimeSpan? interval, CronExpression? cron, string description)
    {
        _interval = interval;
        _cron = cron;
        _description = description;
    }

    /// <summary>Runs every <paramref name="interval"/>, first one interval after startup.</summary>
    public static JobSchedule Every(TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);
        return new JobSchedule(interval, cron: null, $"every {interval}");
    }

    /// <summary>Runs at the times of a five-field cron expression, in Istanbul time.</summary>
    public static JobSchedule Cron(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        return new JobSchedule(
            interval: null,
            CronExpression.Parse(expression),
            $"cron {expression} (Europe/Istanbul)"
        );
    }

    /// <summary>The first run strictly after <paramref name="instant"/>.</summary>
    public DateTimeOffset NextAfter(DateTimeOffset instant) =>
        _interval is { } interval
            ? instant + interval
            : _cron!.GetNextOccurrence(instant, IstanbulCalendar.TimeZone)
                ?? throw new InvalidOperationException($"The schedule {_description} has no next run.");

    /// <inheritdoc />
    public override string ToString() => _description;
}
