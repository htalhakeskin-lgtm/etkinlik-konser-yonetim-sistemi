using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The opaque cursor of a list ordered by time and identifier (api §6.1): the last row's time and id,
/// encoded so the client never reads into it. A record added later never shifts the next slice.
/// </summary>
public static class TimeAndIdCursor
{
    /// <summary>The cursor that continues after the row with this time and identifier.</summary>
    public static string Encode(DateTimeOffset time, Guid id) =>
        Base64Url.EncodeToString(
            Encoding.ASCII.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{time.UtcTicks}.{id:N}"))
        );

    /// <summary>Reads a cursor this class made; anything else is refused.</summary>
    public static bool TryDecode(string cursor, out DateTimeOffset time, out Guid id)
    {
        time = default;
        id = default;
        byte[] bytes;
        try
        {
            bytes = Base64Url.DecodeFromChars(cursor);
        }
        catch (FormatException)
        {
            return false;
        }

        string[] parts = Encoding.ASCII.GetString(bytes).Split('.');
        if (
            parts.Length != 2
            || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long ticks)
            || ticks > DateTimeOffset.MaxValue.UtcTicks
            || !Guid.TryParseExact(parts[1], "N", out id)
        )
        {
            return false;
        }

        time = new DateTimeOffset(ticks, TimeSpan.Zero);
        return true;
    }
}
