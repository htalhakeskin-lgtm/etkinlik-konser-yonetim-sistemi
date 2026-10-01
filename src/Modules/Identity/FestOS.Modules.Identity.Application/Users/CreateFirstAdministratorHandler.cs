using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class CreateFirstAdministratorHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    ITemporaryPasswordGenerator passwords
) : ICommandHandler<CreateFirstAdministratorCommand, string?>
{
    public async Task<string?> HandleAsync(CreateFirstAdministratorCommand command, CancellationToken cancellationToken)
    {
        if (await users.AnyActiveWithRoleAsync(Role.SystemAdministrator, cancellationToken))
        {
            return null;
        }

        string temporaryPassword = passwords.Generate();
        users.Add(
            User.Create(command.FullName, command.Email, [Role.SystemAdministrator], [], hasher.Hash(temporaryPassword))
        );
        return temporaryPassword;
    }
}
