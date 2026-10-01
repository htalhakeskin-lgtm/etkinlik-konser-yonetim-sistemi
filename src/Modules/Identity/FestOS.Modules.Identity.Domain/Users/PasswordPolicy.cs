using System.Globalization;
using System.Text;

namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>
/// The rules a new password meets (BR-SYS-007, security §5.1): long enough, no blanks, neither the user's
/// email nor the product's name in it. No mix of character kinds is required. The common password list
/// and the current password are checked by the caller, which has them.
/// </summary>
public static class PasswordPolicy
{
    /// <summary>The longest password.</summary>
    public const int MaxLength = 128;

    /// <summary>The lowest P-04 the settings accept.</summary>
    public const int LowestMinLength = 8;

    private const string ProductName = "festos";

    /// <summary>The form a password is hashed and compared in: Unicode NFC, so the same text typed on another keyboard matches.</summary>
    public static string Normalize(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return password.Normalize(NormalizationForm.FormC);
    }

    /// <summary>The first rule the normalized password breaks, or <see langword="null"/>; lengths count characters, not bytes.</summary>
    public static PasswordProblem? FindProblem(string password, string email, int minLength)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(email);
        int length = password.EnumerateRunes().Count();
        string lowered = password.ToLower(CultureInfo.InvariantCulture);

        if (length < minLength)
        {
            return PasswordProblem.TooShort;
        }

        if (length > MaxLength)
        {
            return PasswordProblem.TooLong;
        }

        if (password.EnumerateRunes().Any(Rune.IsWhiteSpace))
        {
            return PasswordProblem.Whitespace;
        }

        if (email.Length > 0 && lowered.Contains(EmailAddress.Normalize(email), StringComparison.Ordinal))
        {
            return PasswordProblem.ContainsEmail;
        }

        return lowered.Contains(ProductName, StringComparison.Ordinal) ? PasswordProblem.ContainsProductName : null;
    }
}
