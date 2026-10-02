using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Parties.Application.Parties;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>The query string of <c>GET /api/v1/parties</c>.</summary>
/// <param name="Q">Part of a name or contact point; letter case, Turkish marks and phone spacing do not matter.</param>
/// <param name="Role">Only parties with this role.</param>
/// <param name="Kind">Only people or only organizations.</param>
/// <param name="Status">Active (the default), inactive or all parties.</param>
/// <param name="Sort">Order: <c>name</c> or <c>createdAt</c>; a leading <c>-</c> reverses it.</param>
/// <param name="Page">The page, starting at 1.</param>
/// <param name="PageSize">Rows per page, 25 by default and at most 100.</param>
public sealed record ListPartiesRequest(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "role")] EnumQueryValue<PartyRole>? Role,
    [FromQuery(Name = "kind")] EnumQueryValue<PartyKind>? Kind,
    [FromQuery(Name = "status")] EnumQueryValue<PartyStatusFilter>? Status,
    [FromQuery(Name = "sort")] string? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize
);
