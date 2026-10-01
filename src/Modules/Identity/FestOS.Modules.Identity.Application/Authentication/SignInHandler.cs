using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Authentication;

internal sealed class SignInHandler(IUserRepository users, IPasswordHasher hasher)
    : ICommandHandler<SignInCommand, SignedInUserDetails>
{
    public const string InvalidCredentials = "invalidCredentials";

    public async Task<SignedInUserDetails> HandleAsync(SignInCommand command, CancellationToken cancellationToken)
    {
        User? user = await users.FindByEmailAsync(EmailAddress.Normalize(command.Email), cancellationToken);

        // The answer never tells which of the two was wrong (US-SYS-010).
        if (
            user is null
            || user.DeactivatedAt is not null
            || !hasher.Verify(user.PasswordHash, command.Password, out bool needsRehash)
        )
        {
            throw new AuthenticationFailedException(InvalidCredentials, "The email or the password is wrong.");
        }

        if (needsRehash)
        {
            user.Rehash(hasher.Hash(command.Password));
        }

        return SignedInUserDetails.Of(user);
    }
}
