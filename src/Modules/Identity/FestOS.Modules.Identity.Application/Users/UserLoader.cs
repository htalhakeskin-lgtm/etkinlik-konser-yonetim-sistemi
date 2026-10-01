using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Loads the user a management command changes, at the version the request saw (api §9).</summary>
internal static class UserLoader
{
    public static async Task<User> LoadForChangeAsync(
        this IUserRepository users,
        UserId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        // The system user is no account to manage (ID-11).
        User user =
            id.Value == SystemUser.Id
                ? throw new NotFoundException("User", id.Value)
                : await users.FindAsync(id, cancellationToken) ?? throw new NotFoundException("User", id.Value);
        expectedVersion.EnsureMatches(user);
        return user;
    }
}
