using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>
/// The users list (US-SYS-001): searched by name or email, filtered by role and status, one numbered
/// page at a time. The system user is never listed (ID-11).
/// </summary>
public sealed record ListUsersQuery(string? Q, Role? Role, UserStatusFilter Status, string? Sort, PageRequest Page)
    : IQuery<PagedResult<UserListItem>>
{
    /// <summary>The fields <see cref="Sort"/> may name.</summary>
    public static IReadOnlyList<string> SortFields { get; } = ["fullName", "email", "createdAt"];

    /// <summary>The order without a <see cref="Sort"/>.</summary>
    public const string DefaultSort = "fullName";

    /// <summary>The longest search text.</summary>
    public const int MaxSearchLength = 100;
}
