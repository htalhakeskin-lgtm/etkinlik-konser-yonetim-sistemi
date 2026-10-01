using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Authentication;

/// <summary>The user's sessions, changed in the command's transaction (security §3.5).</summary>
public interface IUserSessions
{
    /// <summary>Ends every other session of the user and brings the current one up to date with the user.</summary>
    Task KeepOnlyCurrentAsync(User user, CancellationToken cancellationToken);
}
