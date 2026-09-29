using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.BuildingBlocks.UnitTests.Paging;

public sealed class PageRequestTests
{
    private readonly PageRequestValidator _validator = new();

    [Fact]
    public void Constructor_WithoutArguments_ReturnsTheFirstPageOfTwentyFive()
    {
        var request = new PageRequest();

        request.Page.ShouldBe(1);
        request.PageSize.ShouldBe(25);
        request.Skip.ShouldBe(0);
    }

    [Fact]
    public void Skip_ForTheThirdPage_SkipsTwoPages() => new PageRequest(3, 20).Skip.ShouldBe(40);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    [InlineData(12, 25)]
    public void Validate_WithinLimits_Passes(int page, int pageSize) =>
        _validator.Validate(new PageRequest(page, pageSize)).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(0, 25, "Page")]
    [InlineData(1, 0, "PageSize")]
    [InlineData(1, 101, "PageSize")]
    public void Validate_OutsideLimits_ReportsTheField(int page, int pageSize, string field) =>
        _validator.Validate(new PageRequest(page, pageSize)).Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(field);
}
