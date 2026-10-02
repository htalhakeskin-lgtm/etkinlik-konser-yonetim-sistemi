namespace FestOS.Modules.Catalog.Contracts;

/// <summary>A kit as other modules see it (catalog §7).</summary>
/// <param name="Id">The kit.</param>
/// <param name="Name">Its name.</param>
/// <param name="IsActive">Whether it can be chosen for new work (BR-SYS-001).</param>
public sealed record KitSummary(Guid Id, string Name, bool IsActive);
