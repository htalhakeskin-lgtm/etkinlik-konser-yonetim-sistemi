using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Audit.Infrastructure.Entries;

/// <summary>The query string of <c>GET /api/v1/audit-entries</c>.</summary>
/// <param name="ActorId">Only the changes of this user.</param>
/// <param name="From">From this Istanbul day.</param>
/// <param name="To">Up to, not including, this Istanbul day.</param>
/// <param name="Module">Only this module's records, by its schema name, e.g. <c>identity</c>.</param>
/// <param name="RootType">Only this type of record, its parts included, e.g. <c>Party</c>.</param>
/// <param name="RootId">Only this record and its parts, e.g. a party with its contact points.</param>
/// <param name="After">The cursor of the previous slice.</param>
/// <param name="Limit">Rows per slice, 50 by default and at most 100.</param>
public sealed record ListAuditEntriesRequest(
    [FromQuery(Name = "actorId")] Guid? ActorId,
    [FromQuery(Name = "from")] DateOnly? From,
    [FromQuery(Name = "to")] DateOnly? To,
    [FromQuery(Name = "module")] string? Module,
    [FromQuery(Name = "rootType")] string? RootType,
    [FromQuery(Name = "rootId")] Guid? RootId,
    [FromQuery(Name = "after")] string? After,
    [FromQuery(Name = "limit")] int? Limit
);
