using System.Globalization;
using System.Text;

namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>
/// Brings an email to the one form it is stored and compared in: trimmed, Unicode NFC and lower case,
/// so the same address typed differently is the same user (BR-SYS-015, database §13).
/// </summary>
public static class EmailAddress
{
    /// <summary>The longest address (RFC 5321).</summary>
    public const int MaxLength = 320;

    /// <summary>The stored form of the address.</summary>
    public static string Normalize(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Trim().Normalize(NormalizationForm.FormC).ToLower(CultureInfo.InvariantCulture);
    }
}
