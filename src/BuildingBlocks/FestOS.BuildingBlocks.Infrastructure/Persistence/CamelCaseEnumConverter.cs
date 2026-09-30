using System.Collections.Frozen;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Stores an enum as the same camelCase text the API uses, e.g. <c>holdPlaced</c> (database §6.2,
/// naming §6).
/// </summary>
internal sealed class CamelCaseEnumConverter<TEnum>()
    : ValueConverter<TEnum, string>(value => ToText(value), text => FromText(text))
    where TEnum : struct, Enum
{
    private static readonly FrozenDictionary<TEnum, string> Texts = Enum.GetValues<TEnum>()
        .ToFrozenDictionary(value => value, value => JsonNamingPolicy.CamelCase.ConvertName(value.ToString()));

    private static readonly FrozenDictionary<string, TEnum> Values = Texts.ToFrozenDictionary(
        pair => pair.Value,
        pair => pair.Key,
        StringComparer.Ordinal
    );

    /// <summary>Every value as stored, for the column's <c>CHECK</c> constraint.</summary>
    public static IEnumerable<string> StoredValues => Texts.Values.Order(StringComparer.Ordinal);

    private static string ToText(TEnum value) => Texts[value];

    private static TEnum FromText(string text) => Values[text];
}
