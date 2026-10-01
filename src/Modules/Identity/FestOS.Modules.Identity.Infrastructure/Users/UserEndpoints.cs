using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Identity.Infrastructure.Users;

/// <summary>The users of the system administrator's screens (identity §7).</summary>
internal static class UserEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/users", ListUsersAsync)
            .RequirePermission(IdentityPermissions.ViewUsers)
            .WithName("ListUsers")
            .WithSummary("Lists the users, searched and filtered, one page at a time.");
        endpoints
            .MapGet("/users/{userId:guid}", GetUserAsync)
            .RequirePermission(IdentityPermissions.ViewUsers)
            .WithName("GetUser")
            .WithSummary("Gets a user.");
    }

    private static async Task<Ok<PagedResult<UserListItem>>> ListUsersAsync(
        [AsParameters] ListUsersRequest request,
        IQueryHandler<ListUsersQuery, PagedResult<UserListItem>> listUsers,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await listUsers.HandleAsync(
                new ListUsersQuery(
                    request.Q,
                    request.Role?.Value,
                    request.Status?.Value ?? UserStatusFilter.Active,
                    request.Sort,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<UserDetails>>> GetUserAsync(
        Guid userId,
        IQueryHandler<GetUserQuery, UserDetails> getUser,
        CancellationToken cancellationToken
    )
    {
        UserDetails user = await getUser.HandleAsync(new GetUserQuery(UserId.From(userId)), cancellationToken);
        return TypedResults.Ok(user).WithVersion(user.Version);
    }
}
