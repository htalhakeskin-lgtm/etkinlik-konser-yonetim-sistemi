using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Users;

internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    public void Add(User user) => context.Users.Add(user);

    public Task<bool> AnyActiveWithRoleAsync(Role role, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.DeactivatedAt == null && user.Roles.Contains(role), cancellationToken);
}
