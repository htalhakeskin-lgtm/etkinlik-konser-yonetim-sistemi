namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>Why a new password is refused (BR-SYS-007).</summary>
public enum PasswordProblem
{
    /// <summary>Shorter than P-04.</summary>
    TooShort,

    /// <summary>Longer than the most the policy allows.</summary>
    TooLong,

    /// <summary>Contains a space or another blank character.</summary>
    Whitespace,

    /// <summary>Contains the user's email.</summary>
    ContainsEmail,

    /// <summary>Contains the product's name.</summary>
    ContainsProductName,

    /// <summary>On the list of common passwords.</summary>
    Common,

    /// <summary>The same as the current password.</summary>
    SameAsCurrent,
}
