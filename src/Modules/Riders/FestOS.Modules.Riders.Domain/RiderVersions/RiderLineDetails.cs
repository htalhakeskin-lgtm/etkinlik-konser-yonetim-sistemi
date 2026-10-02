namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>A rider line as the form sends it (US-RDR-001).</summary>
/// <param name="LineKey">The line's identity across versions; empty for a new line (riders RD-03).</param>
/// <param name="ModelId">The model, when the line asks for one.</param>
/// <param name="CategoryId">The category, when any of its models will do.</param>
/// <param name="Quantity">How many.</param>
/// <param name="Flexibility">On a model line, whether equivalents will do.</param>
/// <param name="EquivalentModelIds">A flexible model line's equivalents, in order of preference.</param>
/// <param name="Note">What the artist adds, such as a use or a setting.</param>
public sealed record RiderLineDetails(
    Guid? LineKey,
    Guid? ModelId,
    Guid? CategoryId,
    int Quantity,
    RiderLineFlexibility? Flexibility,
    IReadOnlyList<Guid> EquivalentModelIds,
    string? Note
);
