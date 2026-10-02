namespace FestOS.Modules.Catalog.Contracts;

/// <summary>A category as other modules see it (catalog §7).</summary>
/// <param name="Id">The category.</param>
/// <param name="Name">Its name.</param>
/// <param name="Path">Its names from the top down, itself last.</param>
/// <param name="IsActive">Whether it can be chosen for new work (BR-SYS-001).</param>
public sealed record CategorySummary(Guid Id, string Name, IReadOnlyList<string> Path, bool IsActive);
