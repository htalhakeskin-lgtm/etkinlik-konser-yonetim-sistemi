using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;
using FluentValidation.Results;

namespace FestOS.BuildingBlocks.UnitTests.Paging;

public sealed class SortSpecTests
{
    private static readonly string[] AllowedFields = ["startsAt", "name"];

    [Fact]
    public void Parse_WithSeveralFields_KeepsOrderAndDirection()
    {
        var sort = SortSpec.Parse("-startsAt, name", "name");

        sort.Fields.ShouldBe([new SortField("startsAt", Descending: true), new SortField("name", Descending: false)]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Parse_WhenEmpty_UsesTheDefault(string? text) =>
        SortSpec.Parse(text, "-startsAt").Fields.ShouldBe([new SortField("startsAt", Descending: true)]);

    [Theory]
    [InlineData(null)]
    [InlineData("name")]
    [InlineData("-startsAt,name")]
    public void IsValid_ForAllowedFields_ReturnsTrue(string? text) =>
        SortSpec.IsValid(text, AllowedFields).ShouldBeTrue();

    [Theory]
    [InlineData("status")]
    [InlineData("Name")]
    [InlineData("name,name")]
    [InlineData("name,")]
    [InlineData("-")]
    public void IsValid_ForUnknownRepeatedOrEmptyFields_ReturnsFalse(string text) =>
        SortSpec.IsValid(text, AllowedFields).ShouldBeFalse();

    [Fact]
    public void SortableBy_WithUnknownField_ReportsTheCodeAndTheAllowedFields()
    {
        var validator = new InlineValidator<SampleListQuery>();
        validator.RuleFor(query => query.Sort).SortableBy(AllowedFields);

        ValidationFailure failure = validator.Validate(new SampleListQuery("status")).Errors.ShouldHaveSingleItem();

        failure.ErrorCode.ShouldBe("unsupportedSort");
        failure.FormattedMessagePlaceholderValues["AllowedFields"].ShouldBe("startsAt,name");
    }

    public sealed record SampleListQuery(string? Sort);
}
