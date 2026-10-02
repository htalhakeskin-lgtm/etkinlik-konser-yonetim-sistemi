using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Venues.Application.Equipment;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

/// <summary>The query string of <c>GET /api/v1/venues/{venueId}/equipment</c>.</summary>
/// <param name="Status">Lines not ended (the default), ended lines, or all.</param>
public sealed record ListVenueEquipmentRequest(
    [FromQuery(Name = "status")] EnumQueryValue<EquipmentStatusFilter>? Status
);
