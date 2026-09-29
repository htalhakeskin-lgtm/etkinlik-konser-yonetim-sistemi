using FestOS.BuildingBlocks.Domain.Time;

namespace FestOS.BuildingBlocks.UnitTests.Time;

public sealed class IstanbulCalendarTests
{
    [Fact]
    public void ToDate_AfterIstanbulMidnight_ReturnsTheNextDay()
    {
        var instant = new DateTimeOffset(2027, 6, 12, 21, 30, 0, TimeSpan.Zero);

        IstanbulCalendar.ToDate(instant).ShouldBe(new DateOnly(2027, 6, 13));
    }

    [Fact]
    public void ToDate_BeforeIstanbulMidnight_ReturnsTheSameDay()
    {
        var instant = new DateTimeOffset(2027, 6, 12, 20, 59, 0, TimeSpan.Zero);

        IstanbulCalendar.ToDate(instant).ShouldBe(new DateOnly(2027, 6, 12));
    }

    [Fact]
    public void Day_ForADate_SpansIstanbulMidnightToMidnightInUtc()
    {
        TimeRange day = IstanbulCalendar.Day(new DateOnly(2027, 6, 13));

        day.Start.ShouldBe(new DateTimeOffset(2027, 6, 12, 21, 0, 0, TimeSpan.Zero));
        day.End.ShouldBe(new DateTimeOffset(2027, 6, 13, 21, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Day_AtTheEndOfTheYear_EndsInTheNextYear()
    {
        TimeRange day = IstanbulCalendar.Day(new DateOnly(2027, 12, 31));

        IstanbulCalendar.ToDate(day.End).ShouldBe(new DateOnly(2028, 1, 1));
        day.Duration.ShouldBe(TimeSpan.FromDays(1));
    }
}
