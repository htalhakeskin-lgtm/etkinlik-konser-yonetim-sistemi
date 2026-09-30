using Cronos;
using FestOS.BuildingBlocks.Infrastructure.Jobs;

namespace FestOS.BuildingBlocks.UnitTests.Jobs;

public sealed class JobScheduleTests
{
    [Fact]
    public void NextAfter_ForAnInterval_AddsTheInterval()
    {
        var now = new DateTimeOffset(2027, 6, 12, 10, 0, 0, TimeSpan.Zero);

        JobSchedule.Every(TimeSpan.FromMinutes(5)).NextAfter(now).ShouldBe(now.AddMinutes(5));
    }

    [Fact]
    public void Every_WithoutAPositiveInterval_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() => JobSchedule.Every(TimeSpan.Zero));

    [Fact]
    public void NextAfter_ForADailyCron_ReadsTheTimeInIstanbul()
    {
        // 00:30 in Istanbul on 13 June; the next 03:00 in Istanbul is 00:00 UTC.
        var justAfterIstanbulMidnight = new DateTimeOffset(2027, 6, 12, 21, 30, 0, TimeSpan.Zero);

        JobSchedule
            .Cron("0 3 * * *")
            .NextAfter(justAfterIstanbulMidnight)
            .ShouldBe(new DateTimeOffset(2027, 6, 13, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void NextAfter_AtARunTime_ReturnsTheFollowingRun()
    {
        var runTime = new DateTimeOffset(2027, 6, 13, 0, 0, 0, TimeSpan.Zero);

        JobSchedule.Cron("0 3 * * *").NextAfter(runTime).ShouldBe(runTime.AddDays(1));
    }

    [Fact]
    public void Cron_WithAnInvalidExpression_Throws() =>
        Should.Throw<CronFormatException>(() => JobSchedule.Cron("every day"));
}
