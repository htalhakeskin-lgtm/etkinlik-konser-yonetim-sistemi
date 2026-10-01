using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class ResetUserPasswordHandler(
    IUserRepository users,
    IUserSessions sessions,
    IPasswordHasher hasher,
    ITemporaryPasswordGenerator passwords,
    ExpectedVersion expectedVersion
) : ICommandHandler<ResetUserPasswordCommand, TemporaryPasswordIssued>
{
    public async Task<TemporaryPasswordIssued> HandleAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        User user = await users.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        string temporaryPassword = passwords.Generate();
        user.ResetPassword(hasher.Hash(temporaryPassword));
        await sessions.EndAllAsync(user.Id, cancellationToken);
        return new TemporaryPasswordIssued(temporaryPassword);
    }
}
