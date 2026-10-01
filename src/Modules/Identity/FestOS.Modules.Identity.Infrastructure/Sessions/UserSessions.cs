using System.Security.Claims;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

// The cached copies are forgotten before the commit; a request in between reloads the row, at worst for
// the cache's 30 seconds (ID-03).
internal sealed class UserSessions(IdentityDbContext context, SessionTicketStore store, IHttpContextAccessor http)
    : IUserSessions
{
    public async Task KeepOnlyCurrentAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        string? current = http.HttpContext?.User.FindFirstValue(IdentityClaims.SessionId);
        foreach (Session session in await SessionsOfAsync(user.Id, cancellationToken))
        {
            if (string.Equals(session.Id.ToString(), current, StringComparison.Ordinal))
            {
                BringUpToDate(session, user);
            }
            else
            {
                context.Remove(session);
            }
        }
    }

    public async Task RefreshAllAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        foreach (Session session in await SessionsOfAsync(user.Id, cancellationToken))
        {
            BringUpToDate(session, user);
        }
    }

    public async Task EndAllAsync(UserId user, CancellationToken cancellationToken)
    {
        foreach (Session session in await SessionsOfAsync(user, cancellationToken))
        {
            context.Remove(session);
        }
    }

    private async Task<List<Session>> SessionsOfAsync(UserId user, CancellationToken cancellationToken)
    {
        List<Session> sessions = await context
            .Set<Session>()
            .Where(session => session.UserId == user)
            .ToListAsync(cancellationToken);
        foreach (Session session in sessions)
        {
            store.Forget(session.KeyHash);
        }

        return sessions;
    }

    private static void BringUpToDate(Session session, User user)
    {
        var details = SignedInUserDetails.Of(user);
        session.Permissions = [.. details.Permissions];
        session.WarehouseIds = [.. details.WarehouseIds];
        session.MustChangePassword = details.MustChangePassword;
    }
}
