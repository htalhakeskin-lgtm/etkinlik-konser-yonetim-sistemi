namespace FestOS.Modules.Identity.Application.Authentication;

/// <summary>
/// The outcome of a sign-in. A refusal is a result, not an exception, so the failed count and the attempt
/// are saved with it.
/// </summary>
public sealed record SignInResult
{
    private SignInResult(SignedInUserDetails? user, DateTimeOffset? lockedUntil)
    {
        User = user;
        LockedUntil = lockedUntil;
    }

    /// <summary>A wrong email or password; the answer does not tell which (US-SYS-010).</summary>
    public static SignInResult Refused { get; } = new(null, null);

    /// <summary>The signed-in user; <see langword="null"/> when refused.</summary>
    public SignedInUserDetails? User { get; }

    /// <summary>Until when the account is locked, when that is why it was refused (BR-SYS-005).</summary>
    public DateTimeOffset? LockedUntil { get; }

    /// <summary>The user signed in.</summary>
    public static SignInResult SignedIn(SignedInUserDetails user) => new(user, null);

    /// <summary>The account is locked until the given time.</summary>
    public static SignInResult Locked(DateTimeOffset lockedUntil) => new(null, lockedUntil);
}
