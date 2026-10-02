namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>A venue's equipment; <see cref="Version"/> is the venue's, sent back with <c>If-Match</c> (venues VN-04).</summary>
public sealed record VenueEquipmentList(IReadOnlyList<VenueEquipmentItem> Lines, int Version);
