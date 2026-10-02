using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Venues.Application.Venues;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

/// <summary>The query string of <c>GET /api/v1/venues</c>.</summary>
/// <param name="Q">Part of a name or city; letter case and Turkish marks do not matter.</param>
/// <param name="City">Only venues in this city.</param>
/// <param name="Status">Active (the default), inactive or all venues.</param>
/// <param name="Sort">Order: <c>name</c>, <c>city</c> or <c>capacity</c>; a leading <c>-</c> reverses it.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListVenuesRequest(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "city")] string? City,
    [FromQuery(Name = "status")] EnumQueryValue<VenueStatusFilter>? Status,
    [FromQuery(Name = "sort")] string? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
