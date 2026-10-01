using System.Text;

namespace FestOS.BuildingBlocks.Domain.Text;

/// <summary>
/// The form text is searched in (database §13): trimmed, Unicode NFC, lower case, and the Turkish
/// letters without their marks, so "isik" finds "Işık" and "IŞIK" alike. Search columns end in
/// <c>_search</c> and hold this key; the query's text gets the same key.
/// </summary>
public static class SearchKey
{
    /// <summary>The search key of the text.</summary>
    public static string Of(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        string normalized = text.Trim().Normalize(NormalizationForm.FormC);
        var key = new StringBuilder(normalized.Length);
        foreach (char letter in normalized)
        {
            // Turkish lower case takes I to ı and İ to i; both then lose their marks, so all three are i.
            key.Append(
                letter switch
                {
                    'I' or 'İ' or 'ı' => 'i',
                    'Ç' or 'ç' => 'c',
                    'Ğ' or 'ğ' => 'g',
                    'Ö' or 'ö' => 'o',
                    'Ş' or 'ş' => 's',
                    'Ü' or 'ü' => 'u',
                    _ => char.ToLowerInvariant(letter),
                }
            );
        }

        return key.ToString();
    }
}
