using System.Text.Json;
using System.Text.Json.Serialization;

namespace FestOS.BuildingBlocks.Infrastructure.Http.Json;

/// <summary>
/// Durations travel as whole minutes, in fields named <c>…Minutes</c> (api §5.2, ADR-0017), instead of
/// .NET's <c>"1.02:00:00"</c> text.
/// </summary>
internal sealed class WholeMinutesJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int minutes)
            ? TimeSpan.FromMinutes(minutes)
            : throw new JsonException("A duration must be a whole number of minutes.");

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            throw new JsonException("A duration sent to the API must be a whole number of minutes.");
        }

        writer.WriteNumberValue((long)value.TotalMinutes);
    }
}
