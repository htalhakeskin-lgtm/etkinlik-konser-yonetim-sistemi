namespace FestOS.Modules.Identity.Application.Passwords;

/// <summary>Makes the temporary passwords the system administrator hands over (BR-SYS-006).</summary>
public interface ITemporaryPasswordGenerator
{
    /// <summary>A new temporary password, e.g. <c>K7QM-3XRA-9PZT-W4HE</c>.</summary>
    string Generate();
}
