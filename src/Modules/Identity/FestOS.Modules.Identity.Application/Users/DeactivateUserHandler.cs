using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class DeactivateUserHandler(
    IUserRepository users,
    IUserSessions sessions,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateUserCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        User user = await users.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        if (user.Id.Value == currentUser.UserId)
        {
            throw new BusinessRuleViolationException(
                IdentityRuleCodes.AdministratorProtection,
                "A system administrator cannot deactivate themselves."
            );
        }

        if (
            user.DeactivatedAt is null
            && user.Roles.Contains(Role.SystemAdministrator)
            && !await users.AnyOtherActiveWithRoleAsync(Role.SystemAdministrator, user.Id, cancellationToken)
        )
        {
            throw new BusinessRuleViolationException(
                IdentityRuleCodes.AdministratorProtection,
                "The last active system administrator cannot be deactivated."
            );
        }

        user.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        await sessions.EndAllAsync(user.Id, cancellationToken);
        return true;
    }
}
