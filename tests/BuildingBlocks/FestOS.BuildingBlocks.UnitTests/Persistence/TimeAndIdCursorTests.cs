using FestOS.BuildingBlocks.Infrastructure.Persistence;

namespace FestOS.BuildingBlocks.UnitTests.Persistence;

public sealed class TimeAndIdCursorTests
{
    [Fact]
    public void Decode_GivesBackWhatWasEncoded()
    {
        DateTimeOffset time = new DateTimeOffset(2027, 1, 4, 6, 30, 15, TimeSpan.Zero).AddTicks(1234);
        var id = Guid.CreateVersion7();

        TimeAndIdCursor
            .TryDecode(TimeAndIdCursor.Encode(time, id), out DateTimeOffset decodedTime, out Guid decodedId)
            .ShouldBeTrue();

        decodedTime.ShouldBe(time);
        decodedId.ShouldBe(id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a cursor")]
    [InlineData("MTIz")]
    public void Decode_RefusesAnythingElse(string cursor) =>
        TimeAndIdCursor.TryDecode(cursor, out _, out _).ShouldBeFalse();
}
