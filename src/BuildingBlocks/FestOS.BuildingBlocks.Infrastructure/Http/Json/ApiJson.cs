using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.BuildingBlocks.Infrastructure.Http.Json;

/// <summary>
/// The API's JSON rules (api §5.1–§5.3): .NET's web defaults tightened with the strict .NET 10 options,
/// plus the value formats of the API. Also used for SignalR messages and the OpenAPI document.
/// </summary>
public static class ApiJson
{
    /// <summary>Applies the rules to the options of the HTTP pipeline.</summary>
    public static void Apply(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Rejected: a field sent twice, an unknown field, null for a non-nullable field, a missing
        // required field; names are case-sensitive camelCase.
        options.AllowDuplicateProperties = false;
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        options.RespectNullableAnnotations = true;
        options.RespectRequiredConstructorParameters = true;
        options.PropertyNameCaseInsensitive = false;
        options.NumberHandling = JsonNumberHandling.Strict;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        options.Converters.Add(new DecimalAsStringJsonConverter());
        options.Converters.Add(new WholeMinutesJsonConverter());
        options.Converters.Add(new CurrencyJsonConverter());
        options.Converters.Add(new StronglyTypedIdJsonConverterFactory());

        options.TypeInfoResolver = (options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(
            NormalizeIncomingText
        );
    }

    // Every text property is trimmed and normalized on the way in, except [Sensitive] ones.
    private static void NormalizeIncomingText(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (
                property.PropertyType == typeof(string)
                && property.AttributeProvider?.IsDefined(typeof(SensitiveAttribute), inherit: true) != true
            )
            {
                property.CustomConverter ??= NormalizedStringJsonConverter.Instance;
            }
        }
    }
}
