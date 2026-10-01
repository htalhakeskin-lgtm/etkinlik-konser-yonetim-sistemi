using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>
/// A person who signs in (05 §5.1). Roles and assigned warehouses are part of the user; a user is never
/// deleted, only deactivated.
/// </summary>
public sealed class User : AggregateRoot<UserId>, IDeactivatable
{
    /// <summary>The longest full name.</summary>
    public const int FullNameMaxLength = 200;

    private List<Role> _roles = [];
    private List<Guid> _warehouseIds = [];

    private User(UserId id)
        : base(id) { }

    /// <summary>The full name.</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>The email in its stored form (<see cref="EmailAddress.Normalize"/>); unique (BR-SYS-015).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>The password hash; never in the change history.</summary>
    [NotAudited]
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Whether the user must set a new password before anything else (BR-SYS-006).</summary>
    public bool MustChangePassword { get; private set; }

    /// <summary>Failed sign-ins since the last success or lock.</summary>
    [NotAudited]
    public int FailedLoginCount { get; private set; }

    /// <summary>Until when sign-in is refused.</summary>
    [NotAudited]
    public DateTimeOffset? LockedUntil { get; private set; }

    /// <summary>The roles; the user's permissions are their union (BR-SYS-002).</summary>
    public IReadOnlyList<Role> Roles => _roles;

    /// <summary>The warehouses a warehouse manager works in.</summary>
    public IReadOnlyList<Guid> WarehouseIds => _warehouseIds;

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>Whether sign-in is refused at <paramref name="now"/>, even with the right password (BR-SYS-005).</summary>
    public bool IsLockedAt(DateTimeOffset now) => LockedUntil > now;

    /// <summary>Counts a wrong password; the last one the policy allows locks the account for a while (BR-SYS-005).</summary>
    public void RecordFailedSignIn(DateTimeOffset now, LockoutPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        FailedLoginCount++;
        if (FailedLoginCount >= policy.MaxFailedAttempts)
        {
            LockedUntil = now + policy.Duration;
            FailedLoginCount = 0;
        }
    }

    /// <summary>Starts the count of wrong passwords again.</summary>
    public void RecordSuccessfulSignIn()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
    }

    /// <summary>Sets a new password, which ends the temporary one (BR-SYS-006); the policy was checked before.</summary>
    public void ChangePassword(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(passwordHash);
        PasswordHash = passwordHash;
        MustChangePassword = false;
    }

    /// <summary>Replaces the hash with one made with the current settings; the password stays the same.</summary>
    public void Rehash(string passwordHash) => PasswordHash = passwordHash;

    /// <summary>A new user who signs in with a temporary password and must replace it (BR-SYS-006).</summary>
    public static User Create(
        string fullName,
        string email,
        IEnumerable<Role> roles,
        IEnumerable<Guid> warehouseIds,
        string temporaryPasswordHash
    ) =>
        new(UserId.New())
        {
            FullName = fullName.Trim(),
            Email = EmailAddress.Normalize(email),
            _roles = [.. roles.Distinct().Order()],
            _warehouseIds = [.. warehouseIds.Distinct().Order()],
            PasswordHash = temporaryPasswordHash,
            MustChangePassword = true,
        };
}
