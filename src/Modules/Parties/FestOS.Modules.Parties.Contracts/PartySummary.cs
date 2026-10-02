namespace FestOS.Modules.Parties.Contracts;

/// <summary>A party as other modules see it: its name, roles and whether it is active (parties §9).</summary>
/// <param name="Id">The party.</param>
/// <param name="Name">The name lists show.</param>
/// <param name="IsActive">Whether it can be chosen for new work (BR-SYS-001).</param>
/// <param name="Roles">Its roles, as <see cref="PartyRoles"/> names them.</param>
public sealed record PartySummary(Guid Id, string Name, bool IsActive, IReadOnlyList<string> Roles)
{
    /// <summary>Whether it can be chosen for a field that needs <paramref name="role"/> (BR-PTY-004).</summary>
    public bool IsSelectableAs(string role) => IsActive && Roles.Contains(role, StringComparer.Ordinal);
}
