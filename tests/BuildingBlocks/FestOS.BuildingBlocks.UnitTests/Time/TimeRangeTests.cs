using CsCheck;
using FestOS.BuildingBlocks.Domain.Time;

namespace FestOS.BuildingBlocks.UnitTests.Time;

public sealed class TimeRangeTests
{
    private static readonly DateTimeOffset Origin = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly Gen<TimeRange> Ranges = Gen.Select(
        Gen.Int[0, 10_000],
        Gen.Int[1, 1_000],
        (startMinutes, lengthMinutes) => At(startMinutes, startMinutes + lengthMinutes)
    );

    [Fact]
    public void Constructor_WhenEndIsNotAfterStart_Throws()
    {
        Should.Throw<ArgumentException>(() => new TimeRange(Origin, Origin));
        Should.Throw<ArgumentException>(() => new TimeRange(Origin, Origin.AddMinutes(-1)));
    }

    [Fact]
    public void Constructor_WhenAnEndIsNotUtc_Throws()
    {
        var local = new DateTimeOffset(2027, 1, 1, 3, 0, 0, TimeSpan.FromHours(3));

        Should.Throw<ArgumentException>(() => new TimeRange(local, local.AddHours(1)));
    }

    [Fact]
    public void Overlaps_WhenRangesOnlyTouch_ReturnsFalse()
    {
        TimeRange first = At(0, 60);
        TimeRange second = At(60, 120);

        first.Overlaps(second).ShouldBeFalse();
    }

    [Fact]
    public void Overlaps_WhenRangesShareAMinute_ReturnsTrue() => At(0, 61).Overlaps(At(60, 120)).ShouldBeTrue();

    [Fact]
    public void Contains_Instant_IncludesStartAndExcludesEnd()
    {
        TimeRange range = At(0, 60);

        range.Contains(range.Start).ShouldBeTrue();
        range.Contains(range.End).ShouldBeFalse();
    }

    [Fact]
    public void Expand_WithBuffers_WidensBothEnds()
    {
        TimeRange expanded = At(60, 120).Expand(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(45));

        expanded.ShouldBe(At(30, 165));
    }

    [Fact]
    public void Expand_WithNegativeBuffer_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => At(0, 60).Expand(TimeSpan.FromMinutes(-1), TimeSpan.Zero));
    }

    [Fact]
    public void Overlaps_ForAnyTwoRanges_IsSymmetric() =>
        Gen.Select(Ranges, Ranges).Sample((first, second) => first.Overlaps(second) == second.Overlaps(first));

    [Fact]
    public void Overlaps_ForAnyRange_IncludesItself() => Ranges.Sample(range => range.Overlaps(range));

    [Fact]
    public void Overlaps_ForAnyRangeAndTheOneRightAfterIt_ReturnsFalse() =>
        Gen.Select(Ranges, Gen.Int[1, 1_000])
            .Sample(
                (range, lengthMinutes) => !range.Overlaps(new TimeRange(range.End, range.End.AddMinutes(lengthMinutes)))
            );

    [Fact]
    public void Contains_ForAnyContainedRange_ImpliesOverlap() =>
        Gen.Select(Ranges, Ranges).Sample((outer, inner) => !outer.Contains(inner) || outer.Overlaps(inner));

    private static TimeRange At(int startMinutes, int endMinutes) =>
        new(Origin.AddMinutes(startMinutes), Origin.AddMinutes(endMinutes));
}
