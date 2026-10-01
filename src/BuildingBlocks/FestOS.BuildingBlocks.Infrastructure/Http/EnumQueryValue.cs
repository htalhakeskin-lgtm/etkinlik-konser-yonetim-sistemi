using System.Diagnostics.CodeAnalysis;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// An enum in the query string, read the way JSON bodies read it: by its camelCase name, whatever the
/// letter case, e.g. <c>?status=inactive</c> (api §6.3). Minimal APIs read a plain enum by its exact C#
/// name, which the OpenAPI document does not show; the document describes this type as the enum's names.
/// </summary>
/// <typeparam name="TEnum">The enum.</typeparam>
public readonly record struct EnumQueryValue<TEnum>(TEnum Value)
    where TEnum : struct, Enum
{
    /// <summary>Reads a name of the enum; numbers and combinations are refused.</summary>
    [SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "Minimal APIs bind a query value through the type's static TryParse."
    )]
    public static bool TryParse(string? text, out EnumQueryValue<TEnum> result)
    {
        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            if (string.Equals(Enum.GetName(value), text?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                result = new EnumQueryValue<TEnum>(value);
                return true;
            }
        }

        result = default;
        return false;
    }
}
