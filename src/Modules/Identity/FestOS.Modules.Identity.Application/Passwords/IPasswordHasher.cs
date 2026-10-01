namespace FestOS.Modules.Identity.Application.Passwords;

/// <summary>Hashes and checks passwords (ADR-0027: PBKDF2-HMAC-SHA512, 210,000 iterations).</summary>
public interface IPasswordHasher
{
    /// <summary>The hash to store.</summary>
    string Hash(string password);

    /// <summary>
    /// Whether the password matches; <paramref name="needsRehash"/> when the hash used older settings. A
    /// missing or empty hash never matches, but takes as long to check as a real one (security §4.1).
    /// </summary>
    bool Verify(string? hash, string password, out bool needsRehash);
}
