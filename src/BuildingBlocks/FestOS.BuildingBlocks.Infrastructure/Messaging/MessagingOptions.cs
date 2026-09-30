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
}
