using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Infrastructure.Riders;

/// <summary>
/// A line of a new version; a line kept from the previous version sends its <c>lineKey</c>, a new one
/// leaves it out (riders RD-03).
/// </summary>
public sealed record RiderLineRequest(
    int Quantity,
    Guid? LineKey = null,
    Guid? ModelId = null,
    Guid? CategoryId = null,
    RiderLineFlexibility? Flexibility = null,
    IReadOnlyList<Guid>? EquivalentModelIds = null,
    string? Note = null
)
{
    /// <summary>The line as the rider takes it.</summary>
    public RiderLineDetails Details() =>
        new(LineKey, ModelId, CategoryId, Quantity, Flexibility, EquivalentModelIds ?? [], Note);
}
