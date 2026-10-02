using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>
/// A venue's equipment lines with their periods and the names of their models and categories
/// (US-VEN-002, parties MD-02); "today" is the venue's day.
/// </summary>
public sealed record ListVenueEquipmentQuery(VenueId VenueId, EquipmentStatusFilter Status)
    : IQuery<VenueEquipmentList>;
