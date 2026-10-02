using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Catalog.Application.Categories;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Catalog.Infrastructure.Categories;

/// <summary>The query string of <c>GET /api/v1/equipment-categories</c>.</summary>
/// <param name="Status">Active (the default), inactive or all categories.</param>
public sealed record ListEquipmentCategoriesRequest(
    [FromQuery(Name = "status")] EnumQueryValue<CategoryStatusFilter>? Status
);
