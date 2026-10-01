using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Users;

internal sealed class ListUsersHandler(IdentityDbContext context, TimeProvider timeProvider)
    : IQueryHandler<ListUsersQuery, PagedResult<UserListItem>>
{
    public static SortKeys<User> SortKeys { get; } =
        new SortKeys<User>()
            .Add("fullName", user => EF.Functions.Collate(user.FullName, Collations.Turkish))
            .Add("email", user => user.Email)
            .Add("createdAt", user => user.CreatedAt);

    public Task<PagedResult<UserListItem>> HandleAsync(ListUsersQuery query, CancellationToken cancellationToken)
    {
        var systemUser = UserId.From(SystemUser.Id);
        DateTimeOffset now = timeProvider.GetUtcNow();
        IQueryable<User> users = context.Users.AsNoTracking().Where(user => user.Id != systemUser);

        users = query.Status switch
        {
            UserStatusFilter.Active => users.Where(user => user.DeactivatedAt == null),
            UserStatusFilter.Inactive => users.Where(user => user.DeactivatedAt != null),
            _ => users,
        };
        if (query.Role is { } role)
        {
            users = users.Where(user => user.Roles.Contains(role));
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            users = users.Where(user => user.FullNameSearch.Contains(key) || user.Email.Contains(key));
        }

        return SortKeys
            .Apply(users, SortSpec.Parse(query.Sort, ListUsersQuery.DefaultSort), user => user.Id)
            .Select(user => new UserListItem(
                user.Id,
                user.FullName,
                user.Email,
                user.Roles,
                user.DeactivatedAt == null,
                user.LockedUntil > now ? user.LockedUntil : null,
                user.Roles.Contains(Role.WarehouseManager) && user.WarehouseIds.Count == 0,
                user.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);
    }
}
