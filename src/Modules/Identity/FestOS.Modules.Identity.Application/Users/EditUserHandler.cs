using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class EditUserHandler(IUserRepository users, IUserSessions sessions, ExpectedVersion expectedVersion)
    : ICommandHandler<EditUserCommand, bool>
{
    public async Task<bool> HandleAsync(EditUserCommand command, CancellationToken cancellationToken)
    {
        User user = await users.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        string oldEmail = user.Email;
        List<string> oldPermissions = [.. SignedInUserDetails.Of(user).Permissions];
        List<Guid> oldWarehouses = [.. user.WarehouseIds];

        bool losesAdministration =
            user.DeactivatedAt is null
            && user.Roles.Contains(Role.SystemAdministrator)
            && !command.Roles.Contains(Role.SystemAdministrator);
        if (
            losesAdministration
            && !await users.AnyOtherActiveWithRoleAsync(Role.SystemAdministrator, user.Id, cancellationToken)
        )
        {
            throw new BusinessRuleViolationException(
                IdentityRuleCodes.AdministratorProtection,
                "The last active system administrator keeps the role."
            );
        }

        user.Edit(command.FullName, command.Email, command.Roles, command.WarehouseIds);

        if (!string.Equals(user.Email, oldEmail, StringComparison.Ordinal))
        {
            await sessions.EndAllAsync(user.Id, cancellationToken);
        }
        else if (
            !SignedInUserDetails.Of(user).Permissions.SequenceEqual(oldPermissions, StringComparer.Ordinal)
            || !user.WarehouseIds.SequenceEqual(oldWarehouses)
        )
        {
            await sessions.RefreshAllAsync(user, cancellationToken);
        }

        return true;
    }
}
