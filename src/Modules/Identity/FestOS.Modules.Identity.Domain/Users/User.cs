using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Domain.Text;

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

    /// <summary>The full name's search key (database §13), kept with the name.</summary>
    [NotAudited]
    public string FullNameSearch { get; private set; } = string.Empty;

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
    )
    {
        var user = new User(UserId.New()) { PasswordHash = temporaryPasswordHash, MustChangePassword = true };
        user.Describe(fullName, email, roles, warehouseIds);
        return user;
    }

    /// <summary>Changes the name, email, roles and warehouses (US-SYS-001); the change history keeps the old values.</summary>
    public void Edit(string fullName, string email, IEnumerable<Role> roles, IEnumerable<Guid> warehouseIds) =>
        Describe(fullName, email, roles, warehouseIds);

    /// <summary>Closes the user's access; the user stays, so the records that name them keep their name (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens the access of a deactivated user again (ID-05).</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }

    /// <summary>
    /// Replaces the password with a temporary one the user must change at the next sign-in, and lifts a
    /// lock (BR-SYS-006).
    /// </summary>
    public void ResetPassword(string temporaryPasswordHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(temporaryPasswordHash);
        PasswordHash = temporaryPasswordHash;
        MustChangePassword = true;
        FailedLoginCount = 0;
        LockedUntil = null;
    }

    private void Describe(string fullName, string email, IEnumerable<Role> roles, IEnumerable<Guid> warehouseIds)
    {
        List<Role> distinctRoles = [.. roles.Distinct().Order()];
        List<Guid> distinctWarehouses = [.. warehouseIds.Distinct().Order()];
        if (distinctRoles.Contains(Role.WarehouseManager) && distinctWarehouses.Count == 0)
        {
            throw new BusinessRuleViolationException(
                IdentityRuleCodes.WarehouseManagerHasWarehouse,
                "A warehouse manager needs at least one warehouse."
            );
        }

        FullName = fullName.Trim();
        FullNameSearch = SearchKey.Of(fullName);
        Email = EmailAddress.Normalize(email);
        _roles = distinctRoles;
        _warehouseIds = distinctWarehouses;
    }
}
