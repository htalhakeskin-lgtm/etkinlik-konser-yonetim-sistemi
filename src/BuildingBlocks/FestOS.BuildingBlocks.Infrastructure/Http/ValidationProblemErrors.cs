using System.Text;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Errors;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Turns validation failures into the <c>errors</c> of api §8.2: the field as a JSON Pointer, a code
/// the front end translates as <c>validation:{code}</c>, and the values the message needs.
/// </summary>
internal static class ValidationProblemErrors
{
    // FluentValidation's built-in rules: our code and how its placeholders map to our parameters.
    private static readonly Dictionary<string, (string Code, (string From, string To)[] Parameters)> Rules = new(
        StringComparer.Ordinal
    )
    {
        ["NotEmptyValidator"] = ("required", []),
        ["NotNullValidator"] = ("required", []),
        ["MaximumLengthValidator"] = ("maxLength", [("MaxLength", "max")]),
        ["MinimumLengthValidator"] = ("minLength", [("MinLength", "min")]),
        ["LengthValidator"] = ("length", [("MinLength", "min"), ("MaxLength", "max")]),
        ["ExactLengthValidator"] = ("length", [("MinLength", "min"), ("MaxLength", "max")]),
        ["InclusiveBetweenValidator"] = ("range", [("From", "min"), ("To", "max")]),
        ["ExclusiveBetweenValidator"] = ("range", [("From", "min"), ("To", "max")]),
        ["GreaterThanOrEqualValidator"] = ("min", [("ComparisonValue", "min")]),
        ["GreaterThanValidator"] = ("greaterThan", [("ComparisonValue", "value")]),
        ["LessThanOrEqualValidator"] = ("max", [("ComparisonValue", "max")]),
        ["LessThanValidator"] = ("lessThan", [("ComparisonValue", "value")]),
        ["EmailValidator"] = ("email", []),
        ["RegularExpressionValidator"] = ("pattern", []),
        ["EnumValidator"] = ("invalidValue", []),
        ["StringEnumValidator"] = ("invalidValue", []),
    };

    public static IReadOnlyList<ValidationProblemError> From(IReadOnlyList<ValidationError> errors) =>
        [.. errors.Select(error => ToProblemError(error))];

    private static ValidationProblemError ToProblemError(ValidationError error)
    {
        string pointer = Pointer(error.PropertyPath);

        if (Rules.TryGetValue(error.Code, out (string Code, (string From, string To)[] Parameters) rule))
        {
            return new ValidationProblemError(
                pointer,
                rule.Code,
                rule.Parameters.Where(parameter => error.Parameters.ContainsKey(parameter.From))
                    .ToDictionary(
                        parameter => parameter.To,
                        parameter => error.Parameters[parameter.From],
                        StringComparer.Ordinal
                    )
            );
        }

        // Other built-in rules have no message of their own; our custom codes pass through.
        return error.Code.EndsWith("Validator", StringComparison.Ordinal)
            ? new ValidationProblemError(pointer, "invalid", new Dictionary<string, object?>(StringComparer.Ordinal))
            : new ValidationProblemError(
                pointer,
                error.Code,
                error.Parameters.ToDictionary(
                    parameter => JsonNamingPolicy.CamelCase.ConvertName(parameter.Key),
                    parameter => parameter.Value,
                    StringComparer.Ordinal
                )
            );
    }

    // "Units[3].SerialNumber" becomes "/units/3/serialNumber" (RFC 6901 escapes ~ and /).
    private static string Pointer(string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath))
        {
            return string.Empty;
        }

        var pointer = new StringBuilder();
        foreach (string segment in propertyPath.Replace("]", string.Empty, StringComparison.Ordinal).Split(['.', '[']))
        {
            string name = JsonNamingPolicy.CamelCase.ConvertName(segment);
            pointer
                .Append('/')
                .Append(name.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal));
        }

        return pointer.ToString();
    }
}
