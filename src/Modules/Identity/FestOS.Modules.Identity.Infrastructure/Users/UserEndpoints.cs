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

        // The answers carry a temporary password, which a stored idempotency result would keep for a day. A
        // repeated create is refused by the unique email; a repeated reset only issues another password.
        endpoints
            .MapPost("/users", CreateUserAsync)
            .RequirePermission(IdentityPermissions.CreateUsers)
            .WithoutIdempotencyKey()
            .WithName("CreateUser")
            .WithSummary("Creates a user; the answer carries the temporary password, once.");
        endpoints
            .MapPost("/users/{userId:guid}/reset-password", ResetUserPasswordAsync)
            .RequirePermission(IdentityPermissions.ResetUserPasswords)
            .RequiresVersion()
            .WithoutIdempotencyKey()
            .WithName("ResetUserPassword")
            .WithSummary("Gives a user a new temporary password, lifts a lock and ends their sessions.");

        endpoints
            .MapPut("/users/{userId:guid}", EditUserAsync)
            .RequirePermission(IdentityPermissions.EditUsers)
            .RequiresVersion()
            .WithName("EditUser")
            .WithSummary("Changes a user's name, email, roles and warehouses.");
        endpoints
            .MapPost("/users/{userId:guid}/deactivate", DeactivateUserAsync)
            .RequirePermission(IdentityPermissions.DeactivateUsers)
            .RequiresVersion()
            .WithName("DeactivateUser")
            .WithSummary("Deactivates a user; their sessions end at once.");
        endpoints
            .MapPost("/users/{userId:guid}/activate", ActivateUserAsync)
            .RequirePermission(IdentityPermissions.DeactivateUsers)
            .RequiresVersion()
            .WithName("ActivateUser")
            .WithSummary("Activates a deactivated user again.");
    }

    private static async Task<Created<UserCreated>> CreateUserAsync(
        UserRequest request,
        ICommandHandler<CreateUserCommand, UserCreated> createUser,
        CancellationToken cancellationToken
    )
    {
        UserCreated created = await createUser.HandleAsync(
            new CreateUserCommand(request.FullName, request.Email, request.Roles, request.WarehouseIds),
            cancellationToken
        );
        return TypedResults.Created($"/api/v1/users/{created.Id.Value}", created);
    }

    private static async Task<VersionedResult<Ok<UserDetails>>> EditUserAsync(
        Guid userId,
        UserRequest request,
        ICommandHandler<EditUserCommand, bool> editUser,
        IQueryHandler<GetUserQuery, UserDetails> getUser,
        CancellationToken cancellationToken
    )
    {
        var id = UserId.From(userId);
        await editUser.HandleAsync(
            new EditUserCommand(id, request.FullName, request.Email, request.Roles, request.WarehouseIds),
            cancellationToken
        );
        return await CurrentAsync(id, getUser, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<UserDetails>>> DeactivateUserAsync(
        Guid userId,
        ICommandHandler<DeactivateUserCommand, bool> deactivateUser,
        IQueryHandler<GetUserQuery, UserDetails> getUser,
        CancellationToken cancellationToken
    )
    {
        var id = UserId.From(userId);
        await deactivateUser.HandleAsync(new DeactivateUserCommand(id), cancellationToken);
        return await CurrentAsync(id, getUser, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<UserDetails>>> ActivateUserAsync(
        Guid userId,
        ICommandHandler<ActivateUserCommand, bool> activateUser,
        IQueryHandler<GetUserQuery, UserDetails> getUser,
        CancellationToken cancellationToken
    )
    {
        var id = UserId.From(userId);
        await activateUser.HandleAsync(new ActivateUserCommand(id), cancellationToken);
        return await CurrentAsync(id, getUser, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<UserPasswordReset>>> ResetUserPasswordAsync(
        Guid userId,
        ICommandHandler<ResetUserPasswordCommand, TemporaryPasswordIssued> resetPassword,
        IQueryHandler<GetUserQuery, UserDetails> getUser,
        CancellationToken cancellationToken
    )
    {
        var id = UserId.From(userId);
        TemporaryPasswordIssued issued = await resetPassword.HandleAsync(
            new ResetUserPasswordCommand(id),
            cancellationToken
        );
        UserDetails user = await getUser.HandleAsync(new GetUserQuery(id), cancellationToken);
        return TypedResults.Ok(new UserPasswordReset(user, issued.TemporaryPassword)).WithVersion(user.Version);
    }

    // After a change the answer is the user as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<UserDetails>>> CurrentAsync(
        UserId id,
        IQueryHandler<GetUserQuery, UserDetails> getUser,
        CancellationToken cancellationToken
    )
    {
        UserDetails user = await getUser.HandleAsync(new GetUserQuery(id), cancellationToken);
        return TypedResults.Ok(user).WithVersion(user.Version);
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
