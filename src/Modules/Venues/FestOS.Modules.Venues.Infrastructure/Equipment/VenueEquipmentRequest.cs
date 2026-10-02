using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

/// <summary>
/// The body of an equipment line: one of a model, a category or a free description, a quantity and optional
/// validity days, the end not included.
/// </summary>
public sealed record VenueEquipmentRequest(
    int Quantity,
    Guid? ModelId = null,
    Guid? CategoryId = null,
    string? Description = null,
    DateOnly? ValidityStart = null,
    DateOnly? ValidityEnd = null
)
{
    /// <summary>The line as the venue takes it.</summary>
    public VenueEquipmentDetails Details() =>
        new(ModelId, CategoryId, Description, Quantity, ValidityStart, ValidityEnd);
}
