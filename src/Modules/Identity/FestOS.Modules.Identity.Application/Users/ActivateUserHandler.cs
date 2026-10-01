using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class ActivateUserHandler(IUserRepository users, ExpectedVersion expectedVersion)
    : ICommandHandler<ActivateUserCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        User user = await users.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        user.Activate();
        return true;
    }
}
