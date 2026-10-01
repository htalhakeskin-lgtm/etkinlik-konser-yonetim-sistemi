using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Users;

internal sealed class GetUserHandler(IdentityDbContext context, TimeProvider timeProvider)
    : IQueryHandler<GetUserQuery, UserDetails>
{
    public async Task<UserDetails> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        var systemUser = UserId.From(SystemUser.Id);
        return await context
                .Users.AsNoTracking()
                .Where(user => user.Id == query.Id && user.Id != systemUser)
                .Select(user => new UserDetails(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Roles,
                    user.WarehouseIds,
                    user.MustChangePassword,
                    user.LockedUntil > now ? user.LockedUntil : null,
                    user.DeactivatedAt,
                    user.CreatedAt,
                    user.UpdatedAt,
                    user.Version
                ))
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", query.Id.Value);
    }
}
