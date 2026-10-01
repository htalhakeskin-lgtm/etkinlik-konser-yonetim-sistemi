using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Authentication;

/// <summary>Records every sign-in attempt in the command's transaction (security §4.1).</summary>
public interface ILoginAttemptLog
{
    /// <summary>Records an attempt: the email in its stored form, and the user when one has it.</summary>
    void Record(string email, UserId? userId, bool succeeded);
}
