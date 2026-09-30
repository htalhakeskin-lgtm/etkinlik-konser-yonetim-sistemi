using System.ComponentModel.DataAnnotations;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Settings of the event delivery, section <c>Messaging</c> (configuration §4).</summary>
public sealed class MessagingOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// How often a dispatcher looks at its outbox without a signal; only a fallback, since every commit
    /// that writes an event wakes the dispatcher at once (ADR-0010).
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "00:10:00")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// P-08: the latency target from an event's occurrence to its delivery; a slower delivery is logged
    /// as a warning (observability §5). The upper limit, P-15, is watched by an alert on the latency metric.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.010", "00:01:00")]
    public TimeSpan EventLatencyTarget { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>When delivered outbox messages and inbox records are cleaned up: a cron expression in Istanbul time.</summary>
    [Required]
    public string CleanupSchedule { get; set; } = "0 4 * * *";

    /// <summary>How long delivered outbox messages and inbox records are kept (database §15).</summary>
    [Range(typeof(TimeSpan), "1.00:00:00", "365.00:00:00")]
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(30);
}
