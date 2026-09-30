using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FestOS.BuildingBlocks.Infrastructure.Http.Json;

/// <summary>
/// Decimals travel as text, e.g. <c>"12.500"</c>, so JavaScript never turns them into binary floating
/// point numbers (api §5.2). A JSON number is refused.
/// </summary>
internal sealed class DecimalAsStringJsonConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("A decimal value must be sent as text.");
        }

        return decimal.TryParse(
            reader.GetString(),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out decimal value
        )
            ? value
            : throw new JsonException("The text is not a decimal value.");
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
