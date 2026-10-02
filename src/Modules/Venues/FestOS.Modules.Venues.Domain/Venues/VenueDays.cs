namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>
/// A venue's days as moments: a day starts at midnight in the venue's time zone (venues VN-01), so the
/// equipment's days compare with an event's moments.
/// </summary>
public static class VenueDays
{
    /// <summary>The moment the day starts at the venue.</summary>
    public static DateTimeOffset StartOf(DateOnly day, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        var midnight = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(midnight, zone), TimeSpan.Zero);
    }
}
