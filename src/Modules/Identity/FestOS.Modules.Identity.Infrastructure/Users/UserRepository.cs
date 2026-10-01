using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

// EF turns == on text into SQL equality; the overloads that take a StringComparison do not translate.
#pragma warning disable MA0006

namespace FestOS.Modules.Identity.Infrastructure.Users;

internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    public Task<User?> FindAsync(UserId id, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public void Add(User user) => context.Users.Add(user);

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

    public Task<bool> AnyActiveWithRoleAsync(Role role, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.DeactivatedAt == null && user.Roles.Contains(role), cancellationToken);
}
