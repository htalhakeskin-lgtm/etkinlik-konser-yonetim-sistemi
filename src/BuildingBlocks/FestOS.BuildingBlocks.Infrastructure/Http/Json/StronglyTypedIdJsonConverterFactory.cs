using System.Text.Json;
using System.Text.Json.Serialization;
using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.BuildingBlocks.Infrastructure.Http.Json;

/// <summary>
/// Writes and reads every strongly typed identifier as its UUID (database §5.2), one converter for all
/// of them.
/// </summary>
internal sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsValueType
        && typeToConvert
            .GetInterfaces()
            .Any(contract =>
                contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>)
                && contract.GetGenericArguments()[0] == typeToConvert
            );

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(StronglyTypedIdJsonConverter<>).MakeGenericType(typeToConvert))!;

    private sealed class StronglyTypedIdJsonConverter<TId> : JsonConverter<TId>
        where TId : struct, IStronglyTypedId<TId>
    {
        public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            TId.From(reader.GetGuid());

        public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value);
    }
}
