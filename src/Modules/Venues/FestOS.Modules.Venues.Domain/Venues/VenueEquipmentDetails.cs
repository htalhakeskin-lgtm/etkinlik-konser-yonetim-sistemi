namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>A venue equipment line as the form sends it (US-VEN-002).</summary>
/// <param name="ModelId">A catalog model, or <see langword="null"/>.</param>
/// <param name="CategoryId">A catalog category, or <see langword="null"/>.</param>
/// <param name="Description">Equipment the catalog lacks, in words; never counted (BR-VEN-001).</param>
/// <param name="Quantity">How many.</param>
/// <param name="ValidityStart">The first day it is there, or open towards the past.</param>
/// <param name="ValidityEnd">The day it stops being there, not included, or open towards the future.</param>
public sealed record VenueEquipmentDetails(
    Guid? ModelId,
    Guid? CategoryId,
    string? Description,
    int Quantity,
    DateOnly? ValidityStart,
    DateOnly? ValidityEnd
);
