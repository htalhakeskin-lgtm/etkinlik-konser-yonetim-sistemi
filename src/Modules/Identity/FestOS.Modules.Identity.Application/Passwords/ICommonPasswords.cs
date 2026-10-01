namespace FestOS.Modules.Identity.Application.Passwords;

/// <summary>The common passwords a new password may not be (BR-SYS-007), checked without an outside service.</summary>
public interface ICommonPasswords
{
    /// <summary>Whether the normalized password is on the list, ignoring case.</summary>
    bool Contains(string password);
}
