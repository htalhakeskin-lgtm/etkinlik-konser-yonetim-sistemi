using System.Text.Json;
using FestOS.BuildingBlocks.Infrastructure.Auditing;

namespace FestOS.Modules.Audit.Infrastructure.Entries;

/// <summary>
/// One change in the history (audit §3): when, who (by the name of that moment), which record, what kind
/// of change, and each changed field's old and new value.
/// </summary>
/// <param name="Id">The entry.</param>
/// <param name="OccurredAt">When the change was saved.</param>
/// <param name="ActorId">The user who made it.</param>
/// <param name="ActorName">The user's name at that moment.</param>
/// <param name="Module">The module that owns the record, by its schema name.</param>
/// <param name="EntityType">The changed record's type, e.g. <c>ContactPoint</c>.</param>
/// <param name="EntityId">The changed record.</param>
/// <param name="RootType">The type of the record it belongs to, e.g. <c>Party</c>; a root names itself.</param>
/// <param name="RootId">The record it belongs to.</param>
/// <param name="Action">Created, updated, deleted or status changed.</param>
/// <param name="Changes">Each changed field with its <c>old</c> and <c>new</c> value.</param>
/// <param name="TraceId">The request's trace, linking the change to its log.</param>
public sealed record AuditEntryItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    string ActorName,
    string Module,
    string EntityType,
    Guid EntityId,
    string RootType,
    Guid RootId,
    AuditAction Action,
    JsonElement Changes,
    string? TraceId
);
