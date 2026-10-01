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

    public string Hash(string password) => _hasher.HashPassword(user: null!, password);

    public bool Verify(string hash, string password, out bool needsRehash)
    {
        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user: null!, hash, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result != PasswordVerificationResult.Failed;
    }
}
