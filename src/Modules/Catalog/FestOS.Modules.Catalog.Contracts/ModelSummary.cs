namespace FestOS.Modules.Catalog.Contracts;

/// <summary>A model as other modules see it (catalog §7).</summary>
/// <param name="Id">The model.</param>
/// <param name="DisplayName">The brand and model name.</param>
/// <param name="CategoryId">Its category.</param>
/// <param name="CategoryPath">The category's names from the top down.</param>
/// <param name="TrackingType">"serialized" or "bulk", as the API names them.</param>
/// <param name="IsActive">Whether it can be chosen for new work (BR-SYS-001).</param>
public sealed record ModelSummary(
    Guid Id,
    string DisplayName,
    Guid CategoryId,
    IReadOnlyList<string> CategoryPath,
    string TrackingType,
    bool IsActive
);
