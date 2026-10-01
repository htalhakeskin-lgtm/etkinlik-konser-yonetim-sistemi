using System.Security.Claims;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

internal sealed class UserSessions(IdentityDbContext context, SessionTicketStore store, IHttpContextAccessor http)
    : IUserSessions
{
    public async Task KeepOnlyCurrentAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        string? current = http.HttpContext?.User.FindFirstValue(IdentityClaims.SessionId);
        var details = SignedInUserDetails.Of(user);

        foreach (
            Session session in await context
                .Set<Session>()
                .Where(session => session.UserId == user.Id)
                .ToListAsync(cancellationToken)
        )
        {
            // Forgotten before the commit; a request in between reloads the row, at worst for the cache's 30 seconds.
            store.Forget(session.KeyHash);
            if (string.Equals(session.Id.ToString(), current, StringComparison.Ordinal))
            {
                session.Permissions = [.. details.Permissions];
                session.WarehouseIds = [.. details.WarehouseIds];
                session.MustChangePassword = details.MustChangePassword;
            }
            else
            {
                context.Remove(session);
            }
        }
    }
}
