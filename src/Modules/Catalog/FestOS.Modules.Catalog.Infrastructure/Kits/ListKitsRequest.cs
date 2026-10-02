using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Catalog.Application.Kits;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

/// <summary>The query string of <c>GET /api/v1/kits</c>.</summary>
/// <param name="Q">Part of a name; letter case and Turkish marks do not matter.</param>
/// <param name="Status">Active (the default), inactive or all kits.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListKitsRequest(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "status")] EnumQueryValue<KitStatusFilter>? Status,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
