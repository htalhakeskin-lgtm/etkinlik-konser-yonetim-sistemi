using System.Runtime.InteropServices;

namespace FestOS.BuildingBlocks.Domain.Time;

/// <summary>
/// A half-open span of time, <c>[Start, End)</c>, in UTC (database §7.3, V-12). Two ranges that only
/// touch do not overlap: an event ending at 18:00 and another starting at 18:00 can share equipment.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct TimeRange
{
    /// <summary>Creates the range; both ends must be UTC and <paramref name="end"/> after <paramref name="start"/>.</summary>
    public TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (start.Offset != TimeSpan.Zero || end.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Time ranges are kept in UTC.", nameof(start));
        }

        if (end <= start)
        {
            throw new ArgumentException("A time range must end after it starts.", nameof(end));
        }

        Start = start;
        End = end;
    }

    /// <summary>The first instant inside the range.</summary>
    public DateTimeOffset Start { get; }

    /// <summary>The first instant after the range.</summary>
    public DateTimeOffset End { get; }

    /// <summary>The length of the range.</summary>
    public TimeSpan Duration => End - Start;

    /// <summary>Whether the two ranges share at least one instant.</summary>
    public bool Overlaps(TimeRange other) => Start < other.End && other.Start < End;

    /// <summary>Whether the instant is inside the range; <see cref="End"/> itself is outside.</summary>
    public bool Contains(DateTimeOffset instant) => Start <= instant && instant < End;

    /// <summary>Whether <paramref name="other"/> lies completely inside this range.</summary>
    public bool Contains(TimeRange other) => Start <= other.Start && other.End <= End;

    /// <summary>
    /// Widens the range, e.g. by the preparation and return buffers around an event (BR-MRP-001).
    /// </summary>
    public TimeRange Expand(TimeSpan before, TimeSpan after)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(before, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(after, TimeSpan.Zero);
        return new TimeRange(Start - before, End + after);
    }
}
