using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

// EF turns == on text into SQL equality; the overloads that take a StringComparison do not translate.
#pragma warning disable MA0006

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>
/// Server-side sessions behind the cookie (ADR-0011, ADR-0027, identity §6). The cookie holds a random
/// key; the database holds its SHA-256. Every request rebuilds the principal from the session row, kept
/// in memory for 30 seconds (ID-03). A session ends after P-03 without a request or P-16 after signing
/// in, whichever comes first (BR-SYS-008).
/// </summary>
internal sealed class SessionTicketStore(
    IServiceScopeFactory scopes,
    IMemoryCache cache,
    TimeProvider timeProvider,
    IdentityModuleOptions options
) : ITicketStore
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LastSeenPrecision = TimeSpan.FromMinutes(1);

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        string key = Base64Url(RandomNumberGenerator.GetBytes(32));
        DateTimeOffset now = timeProvider.GetUtcNow();
        ClaimsPrincipal principal = ticket.Principal;

        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        context.Add(
            new Session
            {
                Id = Guid.CreateVersion7(now),
                UserId = UserId.From(Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!)),
                KeyHash = Hash(key),
                CreatedAt = now,
                LastSeenAt = now,
                ExpiresAt = now + options.SessionAbsoluteLifetime,
                Permissions = [.. principal.FindAll(PermissionClaims.Type).Select(claim => claim.Value)],
                WarehouseIds =
                [
                    .. principal.FindAll(IdentityClaims.Warehouse).Select(claim => Guid.Parse(claim.Value)),
                ],
                MustChangePassword = principal.FindFirst(IdentityClaims.MustChangePassword) is not null,
            }
        );
        await context.SaveChangesAsync();
        return key;
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        string keyHash = Hash(key);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (cache.TryGetValue(keyHash, out CachedSession? cached) && !cached!.HasExpired(now))
        {
            return cached.Ticket;
        }

        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var found = await context
            .Set<Session>()
            .Where(session => session.KeyHash == keyHash)
            .Join(
                context.Users,
                session => session.UserId,
                user => user.Id,
                (session, user) => new { session, user.FullName }
            )
            .SingleOrDefaultAsync();
        if (found is null)
        {
            return null;
        }

        Session session = found.session;
        if (
            new CachedSession(null!, session.ExpiresAt, session.LastSeenAt + options.SessionIdleTimeout).HasExpired(now)
        )
        {
            await context.Set<Session>().Where(expired => expired.Id == session.Id).ExecuteDeleteAsync();
            return null;
        }

        if (now - session.LastSeenAt >= LastSeenPrecision)
        {
            await context
                .Set<Session>()
                .Where(seen => seen.Id == session.Id)
                .ExecuteUpdateAsync(seen => seen.SetProperty(row => row.LastSeenAt, now));
        }

        var ticket = new AuthenticationTicket(
            IdentityClaims.Principal(
                session.UserId.Value,
                found.FullName,
                session.Permissions,
                session.WarehouseIds,
                session.MustChangePassword,
                session.Id
            ),
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        // The cached copy keeps the session's limits, so it never outlives them (BR-SYS-008).
        cache.Set(
            keyHash,
            new CachedSession(
                ticket,
                session.ExpiresAt,
                (now > session.LastSeenAt ? now : session.LastSeenAt) + options.SessionIdleTimeout
            ),
            CacheDuration
        );
        return ticket;
    }

    /// <summary>Drops the cached copy of a session that changed or ended, so the next request reads the row.</summary>
    public void Forget(string keyHash) => cache.Remove(keyHash);

    // The session's lifetime is kept on the server; the renewed cookie carries the same key.
    public Task RenewAsync(string key, AuthenticationTicket ticket) => Task.CompletedTask;

    public async Task RemoveAsync(string key)
    {
        string keyHash = Hash(key);
        cache.Remove(keyHash);
        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        await scope
            .ServiceProvider.GetRequiredService<IdentityDbContext>()
            .Set<Session>()
            .Where(session => session.KeyHash == keyHash)
            .ExecuteDeleteAsync();
    }

    private sealed record CachedSession(AuthenticationTicket Ticket, DateTimeOffset ExpiresAt, DateTimeOffset IdleUntil)
    {
        public bool HasExpired(DateTimeOffset now) => now >= ExpiresAt || now >= IdleUntil;
    }

    private static string Hash(string key) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
