using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Inventory.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

/// <summary>The query string of <c>GET /api/v1/warehouses</c>.</summary>
/// <param name="Q">Part of a name; letter case and Turkish marks do not matter.</param>
/// <param name="Status">Active (the default), inactive or all warehouses.</param>
/// <param name="Sort">Order: <c>name</c> or <c>city</c>; a leading <c>-</c> reverses it.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListWarehousesRequest(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "status")] EnumQueryValue<WarehouseStatusFilter>? Status,
    [FromQuery(Name = "sort")] string? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
