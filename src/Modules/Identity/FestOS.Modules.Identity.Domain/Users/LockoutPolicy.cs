namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>
/// When wrong passwords lock an account (BR-SYS-005): after <paramref name="MaxFailedAttempts"/> in a row
/// (P-01), for <paramref name="Duration"/> (P-02).
/// </summary>
public sealed record LockoutPolicy(int MaxFailedAttempts, TimeSpan Duration);
