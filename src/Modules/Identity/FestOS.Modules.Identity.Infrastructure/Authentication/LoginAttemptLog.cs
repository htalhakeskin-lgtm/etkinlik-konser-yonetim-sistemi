using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

internal sealed class LoginAttemptLog(IdentityDbContext context, IHttpContextAccessor http, TimeProvider timeProvider)
    : ILoginAttemptLog
{
    public void Record(string email, UserId? userId, bool succeeded)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        context.Add(
            new LoginAttempt
            {
                Id = Guid.CreateVersion7(now),
                Email = email,
                UserId = userId,
                OccurredAt = now,
                Succeeded = succeeded,
                IpAddress = http.HttpContext?.Connection.RemoteIpAddress,
            }
        );
    }
}
