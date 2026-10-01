using System.Text.Json;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Passwords;

internal sealed class ChangeMyPasswordHandler(
    IUserRepository users,
    ICurrentUser currentUser,
    IPasswordHasher hasher,
    ICommonPasswords commonPasswords,
    IUserSessions sessions,
    IdentityModuleOptions options
) : ICommandHandler<ChangeMyPasswordCommand, SignedInUserDetails>
{
    /// <summary>The validation code of a wrong current password.</summary>
    public const string IncorrectPassword = "incorrectPassword";

    public async Task<SignedInUserDetails> HandleAsync(
        ChangeMyPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var userId = UserId.From(currentUser.UserId);
        User user =
            await users.FindAsync(userId, cancellationToken) ?? throw new NotFoundException("User", userId.Value);

        // Signing in with the temporary password proved it; otherwise the current one is asked again.
        if (
            !user.MustChangePassword
            && !hasher.Verify(user.PasswordHash, PasswordPolicy.Normalize(command.CurrentPassword ?? ""), out _)
        )
        {
            throw new ValidationFailedException([
                new ValidationError(
                    nameof(command.CurrentPassword),
                    IncorrectPassword,
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                ),
            ]);
        }

        string password = PasswordPolicy.Normalize(command.NewPassword);
        PasswordProblem? problem =
            PasswordPolicy.FindProblem(password, user.Email, options.PasswordMinLength)
            ?? (commonPasswords.Contains(password) ? PasswordProblem.Common : (PasswordProblem?)null)
            ?? (
                hasher.Verify(user.PasswordHash, password, out _)
                    ? PasswordProblem.SameAsCurrent
                    : (PasswordProblem?)null
            );
        if (problem is { } found)
        {
            throw new BusinessRuleViolationException(
                IdentityRuleCodes.PasswordPolicy,
                $"The new password breaks the password policy ({found}).",
                parameters: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["reason"] = JsonNamingPolicy.CamelCase.ConvertName(found.ToString()),
                    ["minLength"] = options.PasswordMinLength,
                }
            );
        }

        user.ChangePassword(hasher.Hash(password));
        await sessions.KeepOnlyCurrentAsync(user, cancellationToken);
        return SignedInUserDetails.Of(user);
    }
}
