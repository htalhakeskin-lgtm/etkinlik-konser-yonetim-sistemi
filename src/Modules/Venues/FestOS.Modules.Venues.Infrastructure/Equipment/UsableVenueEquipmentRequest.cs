using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

/// <summary>The query string of <c>GET /api/v1/venues/{venueId}/equipment/usable</c>: the venue's days, <c>to</c> not included.</summary>
/// <param name="From">The first day.</param>
/// <param name="To">The day after the last one.</param>
public sealed record UsableVenueEquipmentRequest(
    [FromQuery(Name = "from")] DateOnly From,
    [FromQuery(Name = "to")] DateOnly To
);
