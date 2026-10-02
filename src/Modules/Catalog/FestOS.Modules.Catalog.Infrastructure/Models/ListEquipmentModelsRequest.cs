using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Catalog.Application.Models;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Catalog.Infrastructure.Models;

/// <summary>The query string of <c>GET /api/v1/equipment-models</c>.</summary>
/// <param name="Q">Part of the brand and model name; letter case and Turkish marks do not matter.</param>
/// <param name="CategoryId">Only models in this category or below it.</param>
/// <param name="TrackingType">Only serial-numbered or only counted models.</param>
/// <param name="Status">Active (the default), inactive or all models.</param>
/// <param name="Sort">Order: <c>name</c> or <c>brand</c>; a leading <c>-</c> reverses it.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListEquipmentModelsRequest(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "categoryId")] Guid? CategoryId,
    [FromQuery(Name = "trackingType")] EnumQueryValue<TrackingType>? TrackingType,
    [FromQuery(Name = "status")] EnumQueryValue<ModelStatusFilter>? Status,
    [FromQuery(Name = "sort")] string? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
