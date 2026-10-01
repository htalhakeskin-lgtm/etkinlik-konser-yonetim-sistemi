using System.ComponentModel.DataAnnotations;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application;

/// <summary>Settings of signing in, section <c>Modules:Identity</c> (configuration §5).</summary>
public sealed class IdentityModuleOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "Modules:Identity";

    /// <summary>P-01: wrong passwords in a row before the account is locked.</summary>
    [Range(1, 100)]
    public int LockoutMaxFailedAttempts { get; set; } = 5;

    /// <summary>P-02: how long a locked account stays locked.</summary>
    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>P-03: a session ends after this long without a request.</summary>
    [Range(typeof(TimeSpan), "00:05:00", "7.00:00:00")]
    public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromHours(12);

    /// <summary>P-16: a session ends this long after signing in, however much it is used.</summary>
    [Range(typeof(TimeSpan), "01:00:00", "30.00:00:00")]
    public TimeSpan SessionAbsoluteLifetime { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// P-04: the shortest password. Not below <see cref="PasswordPolicy.LowestMinLength"/>, since the common
    /// password list only holds passwords from that length up.
    /// </summary>
    [Range(PasswordPolicy.LowestMinLength, PasswordPolicy.MaxLength)]
    public int PasswordMinLength { get; set; } = 15;

    /// <summary>The lockout settings as the user aggregate takes them.</summary>
    public LockoutPolicy Lockout => new(LockoutMaxFailedAttempts, LockoutDuration);
}
