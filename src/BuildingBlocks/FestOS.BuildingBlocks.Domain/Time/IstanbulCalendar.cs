namespace FestOS.BuildingBlocks.Domain.Time;

/// <summary>
/// Calendar days in the company's time zone. Instants are stored in UTC; a "day" (reports, the
/// warehouse job list, event dates) is always an Istanbul day (database §7.4, ADR-0017).
/// </summary>
public static class IstanbulCalendar
{
    /// <summary>The IANA identifier of the company's time zone.</summary>
    public const string TimeZoneId = "Europe/Istanbul";

    /// <summary>The company's time zone.</summary>
    public static TimeZoneInfo TimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    /// <summary>The Istanbul calendar day that contains the instant.</summary>
    public static DateOnly ToDate(DateTimeOffset instant) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, TimeZone).DateTime);

    /// <summary>The instant (UTC) at which the Istanbul day begins.</summary>
    public static DateTimeOffset StartOfDay(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localMidnight, TimeZone), TimeSpan.Zero);
    }

    /// <summary>The whole Istanbul day as a UTC range, <c>[midnight, next midnight)</c>.</summary>
    public static TimeRange Day(DateOnly date) => new(StartOfDay(date), StartOfDay(date.AddDays(1)));
}
