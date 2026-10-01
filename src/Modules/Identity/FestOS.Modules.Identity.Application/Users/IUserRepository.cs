using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Loads and adds users for commands (identity ID-01); the unit of work saves them.</summary>
public interface IUserRepository
{
    /// <summary>The user with the identifier, or <see langword="null"/>.</summary>
    Task<User?> FindAsync(UserId id, CancellationToken cancellationToken);

    /// <summary>Adds a new user.</summary>
    void Add(User user);

    /// <summary>The user with the email in its stored form, active or not.</summary>
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>Whether an active user has the role.</summary>
    Task<bool> AnyActiveWithRoleAsync(Role role, CancellationToken cancellationToken);
}
