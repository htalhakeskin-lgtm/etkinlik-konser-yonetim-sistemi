using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Riders.Application.Productions;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Riders.Infrastructure.Productions;

/// <summary>The query string of <c>GET /api/v1/productions</c>.</summary>
/// <param name="ArtistId">Only this artist's productions.</param>
/// <param name="Q">Part of a name; letter case and Turkish marks do not matter.</param>
/// <param name="Status">Active (the default), inactive or all productions.</param>
/// <param name="Sort">Order: <c>name</c> or <c>createdAt</c>; a leading <c>-</c> reverses it.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListProductionsRequest(
    [FromQuery(Name = "artistId")] Guid? ArtistId,
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "status")] EnumQueryValue<ProductionStatusFilter>? Status,
    [FromQuery(Name = "sort")] string? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
