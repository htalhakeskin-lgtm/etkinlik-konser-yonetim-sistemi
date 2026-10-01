using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Loads and adds users for commands (identity ID-01); the unit of work saves them.</summary>
public interface IUserRepository
{
    /// <summary>Adds a new user.</summary>
    void Add(User user);

    /// <summary>Whether an active user has the role.</summary>
    Task<bool> AnyActiveWithRoleAsync(Role role, CancellationToken cancellationToken);
}
