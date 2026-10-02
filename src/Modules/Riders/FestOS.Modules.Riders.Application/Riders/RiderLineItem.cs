using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>
/// A line with its target's name and category path (the view groups by the top category, 06 E-01) and
/// whether the target is still active (riders RD-04).
/// </summary>
public sealed record RiderLineItem(
    RiderLineId Id,
    Guid LineKey,
    Guid? ModelId,
    Guid? CategoryId,
    string Name,
    IReadOnlyList<string> CategoryPath,
    bool IsTargetActive,
    int Quantity,
    RiderLineFlexibility? Flexibility,
    IReadOnlyList<RiderEquivalentItem> Equivalents,
    string? Note
);
