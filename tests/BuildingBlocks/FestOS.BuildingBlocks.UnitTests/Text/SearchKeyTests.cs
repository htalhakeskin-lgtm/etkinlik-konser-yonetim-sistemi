using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.BuildingBlocks.UnitTests.Text;

public sealed class SearchKeyTests
{
    [Theory]
    [InlineData("Işık", "isik")]
    [InlineData("IŞIK", "isik")]
    [InlineData("İpek Çağlar", "ipek caglar")]
    [InlineData("  Gülşen Öztürk ", "gulsen ozturk")]
    [InlineData("AYSE@Example.com", "ayse@example.com")]
    public void Of_IgnoresLetterCaseAndTurkishMarks(string text, string key) => SearchKey.Of(text).ShouldBe(key);

    [Fact]
    public void Of_GivesTheSameKeyToTheSameTextTypedDifferently() =>
        SearchKey.Of("Şükrü").ShouldBe(SearchKey.Of("Şükrü"));
}
