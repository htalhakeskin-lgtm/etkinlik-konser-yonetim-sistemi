using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using IdentityPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<FestOS.Modules.Identity.Domain.Users.User>;

namespace FestOS.Modules.Identity.Infrastructure.Passwords;

/// <summary>
/// ASP.NET Core Identity's hasher: PBKDF2-HMAC-SHA512 with 210,000 iterations (ADR-0027). A hash made
/// with older settings verifies and asks to be replaced.
/// </summary>
internal sealed class PasswordHasher : Application.Passwords.IPasswordHasher
{
    public const int Iterations = 210_000;

    private readonly IdentityPasswordHasher _hasher = new(
        Options.Create(
            new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = Iterations,
            }
        )
    );

    // Any hash with the current settings will do; it only makes a check without a user take as long.
    private string? _standInHash;

    public string Hash(string password) => _hasher.HashPassword(user: null!, password);

    public bool Verify(string? hash, string password, out bool needsRehash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            _standInHash ??= Hash(Convert.ToHexString(RandomNumberGenerator.GetBytes(16)));
            _hasher.VerifyHashedPassword(user: null!, _standInHash, password);
            needsRehash = false;
            return false;
        }

        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user: null!, hash, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result != PasswordVerificationResult.Failed;
    }
}
