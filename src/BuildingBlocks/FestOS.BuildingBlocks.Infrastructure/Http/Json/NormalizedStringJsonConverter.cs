using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FestOS.BuildingBlocks.Infrastructure.Http.Json;

/// <summary>
/// Incoming text is trimmed and brought to Unicode NFC, so "İstanbul " typed on two keyboards compares
/// equal (api §5.3, database §13). Outgoing text is written as it is.
/// </summary>
internal sealed class NormalizedStringJsonConverter : JsonConverter<string>
{
    public static readonly NormalizedStringJsonConverter Instance = new();

    public override bool HandleNull => false;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString()?.Trim().Normalize(NormalizationForm.FormC);

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value);
    }
}
