using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.BuildingBlocks.UnitTests.Paging;

public sealed class CursorRequestTests
{
    private readonly CursorRequestValidator _validator = new();

    [Fact]
    public void Constructor_WithoutArguments_ReturnsTheFirstSliceOfFifty()
    {
        var request = new CursorRequest();

        request.After.ShouldBeNull();
        request.Limit.ShouldBe(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithLimitOutsideLimits_ReportsTheLimit(int limit) =>
        _validator
            .Validate(new CursorRequest(Limit: limit))
            .Errors.ShouldHaveSingleItem()
            .PropertyName.ShouldBe("Limit");

    [Fact]
    public void Validate_WithOverlongCursor_ReportsTheCursor() =>
        _validator
            .Validate(new CursorRequest(new string('a', 513)))
            .Errors.ShouldHaveSingleItem()
            .PropertyName.ShouldBe("After");
}
