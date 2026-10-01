using FestOS.BuildingBlocks.Infrastructure.Http;

namespace FestOS.BuildingBlocks.UnitTests.Http;

public sealed class EnumQueryValueTests
{
    public enum Colour
    {
        DarkRed,
        Blue,
    }

    [Theory]
    [InlineData("darkRed", Colour.DarkRed)]
    [InlineData("DARKRED", Colour.DarkRed)]
    [InlineData("blue", Colour.Blue)]
    public void TryParse_ReadsTheNameInAnyLetterCase(string text, Colour colour)
    {
        EnumQueryValue<Colour>.TryParse(text, out EnumQueryValue<Colour> result).ShouldBeTrue();
        result.Value.ShouldBe(colour);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("darkRed,blue")]
    [InlineData("green")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_RefusesNumbersCombinationsAndUnknownNames(string? text) =>
        EnumQueryValue<Colour>.TryParse(text, out _).ShouldBeFalse();
}
