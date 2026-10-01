using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Authentication;

internal sealed class SignInHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    ILoginAttemptLog attempts,
    IdentityModuleOptions options,
    TimeProvider timeProvider
) : ICommandHandler<SignInCommand, SignInResult>
{
    public async Task<SignInResult> HandleAsync(SignInCommand command, CancellationToken cancellationToken)
    {
        string email = EmailAddress.Normalize(command.Email);
        User? user = await users.FindByEmailAsync(email, cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();

        // The password is checked even without a user, so the answer takes as long either way (security §4.1).
        string password = PasswordPolicy.Normalize(command.Password);
        bool passwordMatches = hasher.Verify(user?.PasswordHash, password, out bool needsRehash);
        SignInResult result = Decide(user, passwordMatches, now);
        attempts.Record(email, user?.Id, succeeded: result.User is not null);

        if (result.User is not null && needsRehash)
        {
            user!.Rehash(hasher.Hash(password));
        }

        return result;
    }

    private SignInResult Decide(User? user, bool passwordMatches, DateTimeOffset now)
    {
        if (user is null || user.DeactivatedAt is not null)
        {
            return SignInResult.Refused;
        }

        // A locked account refuses the right password too, and the attempts do not extend the lock (BR-SYS-005).
        if (user.IsLockedAt(now))
        {
            return SignInResult.Locked(user.LockedUntil!.Value);
        }

        if (!passwordMatches)
        {
            user.RecordFailedSignIn(now, options.Lockout);
            return user.IsLockedAt(now) ? SignInResult.Locked(user.LockedUntil!.Value) : SignInResult.Refused;
        }

        user.RecordSuccessfulSignIn();
        return SignInResult.SignedIn(SignedInUserDetails.Of(user));
    }
}
