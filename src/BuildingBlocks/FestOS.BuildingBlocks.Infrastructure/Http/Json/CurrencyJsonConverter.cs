using System.Text.Json;
using System.Text.Json.Serialization;
using FestOS.BuildingBlocks.Domain.Monetary;

namespace FestOS.BuildingBlocks.Infrastructure.Http.Json;

/// <summary>A currency travels as its ISO 4217 code, so an amount reads <c>{ "amount": "1250.00", "currency": "TRY" }</c>.</summary>
internal sealed class CurrencyJsonConverter : JsonConverter<Currency>
{
    public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return Currency.FromCode(reader.GetString() ?? string.Empty);
        }
        catch (ArgumentException exception)
        {
            throw new JsonException(exception.Message, exception);
        }
    }

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Code);
    }
}
