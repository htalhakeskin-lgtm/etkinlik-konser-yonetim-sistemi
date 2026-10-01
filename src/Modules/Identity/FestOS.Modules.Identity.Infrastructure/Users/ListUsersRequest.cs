using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Identity.Infrastructure.Users;

/// <summary>The query string of <c>GET /api/v1/users</c>.</summary>
/// <param name="Q">Part of a name or an email; letter case and Turkish marks do not matter.</param>
/// <param name="Role">Only users with this role.</param>
/// <param name="Status">Active (the default), inactive or all users.</param>
/// <param name="Sort">Order: <c>fullName</c>, <c>email</c> or <c>createdAt</c>; a leading <c>-</c> reverses it.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListUsersRequest(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "role")] EnumQueryValue<Role>? Role,
    [FromQuery(Name = "status")] EnumQueryValue<UserStatusFilter>? Status,
    [FromQuery(Name = "sort")] string? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
