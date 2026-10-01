using System.Security.Claims;
using FestOS.BuildingBlocks.Application.Users;
using Microsoft.AspNetCore.Http;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>
/// The current user of an HTTP request, from the session (identity §6). A request without a session,
/// such as signing in, acts as the system user.
/// </summary>
internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId)
            ? userId
            : SystemUser.Id;
}
