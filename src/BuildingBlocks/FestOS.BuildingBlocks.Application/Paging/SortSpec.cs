namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>
/// A sort order from <c>?sort=-startsAt,name</c>: fields separated by commas, a leading <c>-</c> for
/// descending (api §6.2). Each list endpoint allows a fixed set of fields, checked with
/// <see cref="SortRuleExtensions.SortableBy{T}"/>; the query code appends the identifier as the last
/// field so rows with equal values keep their order between pages.
/// </summary>
public sealed class SortSpec
{
    /// <summary>The validation error code for a field the endpoint does not allow.</summary>
    public const string UnsupportedFieldErrorCode = "unsupportedSort";

    private SortSpec(IReadOnlyList<SortField> fields)
    {
        Fields = fields;
    }

    /// <summary>The fields, most significant first.</summary>
    public IReadOnlyList<SortField> Fields { get; }

    /// <summary>
    /// Parses an already validated sort parameter, falling back to the endpoint's default order when it
    /// is empty.
    /// </summary>
    public static SortSpec Parse(string? text, string defaultSort)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultSort);

        List<SortField>? fields = TryReadFields(string.IsNullOrWhiteSpace(text) ? defaultSort : text);
        return fields is null
            ? throw new ArgumentException($"'{text}' is not a valid sort order.", nameof(text))
            : new SortSpec(fields);
    }

    /// <summary>
    /// Whether the sort parameter is empty, or names only allowed fields, each once and without empty
    /// parts.
    /// </summary>
    public static bool IsValid(string? text, IReadOnlyCollection<string> allowedFields)
    {
        ArgumentNullException.ThrowIfNull(allowedFields);

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        List<SortField>? fields = TryReadFields(text);
        return fields is not null
            && fields.All(field => allowedFields.Contains(field.Name, StringComparer.Ordinal))
            && fields.DistinctBy(field => field.Name, StringComparer.Ordinal).Count() == fields.Count;
    }

    private static List<SortField>? TryReadFields(string text)
    {
        List<SortField> fields = [];

        foreach (string part in text.Split(',', StringSplitOptions.TrimEntries))
        {
            bool descending = part.StartsWith('-');
            string name = descending ? part[1..] : part;
            if (name.Length == 0)
            {
                return null;
            }

            fields.Add(new SortField(name, descending));
        }

        return fields;
    }
}
