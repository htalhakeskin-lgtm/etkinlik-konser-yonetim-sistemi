using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class CreateUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    ITemporaryPasswordGenerator passwords
) : ICommandHandler<CreateUserCommand, UserCreated>
{
    public Task<UserCreated> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        string temporaryPassword = passwords.Generate();
        var user = User.Create(
            command.FullName,
            command.Email,
            command.Roles,
            command.WarehouseIds,
            hasher.Hash(temporaryPassword)
        );
        users.Add(user);
        return Task.FromResult(new UserCreated(user.Id, temporaryPassword));
    }
}
