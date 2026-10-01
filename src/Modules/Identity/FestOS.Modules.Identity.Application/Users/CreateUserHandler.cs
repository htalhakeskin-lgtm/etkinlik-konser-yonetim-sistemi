using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Inventory.Contracts;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class CreateUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    ITemporaryPasswordGenerator passwords,
    IWarehouseDirectory warehouses
) : ICommandHandler<CreateUserCommand, UserCreated>
{
    public async Task<UserCreated> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        await warehouses.EnsureActiveAsync(command.Roles, command.WarehouseIds, cancellationToken);
        string temporaryPassword = passwords.Generate();
        var user = User.Create(
            command.FullName,
            command.Email,
            command.Roles,
            command.WarehouseIds,
            hasher.Hash(temporaryPassword)
        );
        users.Add(user);
        return new UserCreated(user.Id, temporaryPassword);
    }
}
