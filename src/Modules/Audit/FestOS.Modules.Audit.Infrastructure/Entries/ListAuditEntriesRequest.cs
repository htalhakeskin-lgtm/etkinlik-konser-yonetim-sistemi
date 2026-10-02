using Microsoft.AspNetCore.Mvc;

namespace FestOS.Modules.Audit.Infrastructure.Entries;

/// <summary>The query string of <c>GET /api/v1/audit-entries</c>.</summary>
/// <param name="ActorId">Only the changes of this user.</param>
/// <param name="From">From this Istanbul day.</param>
/// <param name="To">Up to, not including, this Istanbul day.</param>
/// <param name="Module">Only this module's records, by its schema name, e.g. <c>identity</c>.</param>
/// <param name="EntityType">Only this type of record, e.g. <c>User</c>.</param>
/// <param name="EntityId">Only this record.</param>
/// <param name="After">The cursor of the previous slice.</param>
/// <param name="Limit">Rows per slice, 50 by default and at most 100.</param>
public sealed record ListAuditEntriesRequest(
    [FromQuery(Name = "actorId")] Guid? ActorId,
    [FromQuery(Name = "from")] DateOnly? From,
    [FromQuery(Name = "to")] DateOnly? To,
    [FromQuery(Name = "module")] string? Module,
    [FromQuery(Name = "entityType")] string? EntityType,
    [FromQuery(Name = "entityId")] Guid? EntityId,
    [FromQuery(Name = "after")] string? After,
    [FromQuery(Name = "limit")] int? Limit
);
