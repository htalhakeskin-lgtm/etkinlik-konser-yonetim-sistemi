using System.Text.Json;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Time;
using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// EF turns == on text into SQL equality; the overloads that take a StringComparison do not translate.
#pragma warning disable MA0006

namespace FestOS.Modules.Audit.Infrastructure.Entries;

internal sealed class ListAuditEntriesHandler(AuditDbContext context)
    : IQueryHandler<ListAuditEntriesQuery, CursorResult<AuditEntryItem>>
{
    public const string InvalidCursorCode = "invalidCursor";

    public async Task<CursorResult<AuditEntryItem>> HandleAsync(
        ListAuditEntriesQuery query,
        CancellationToken cancellationToken
    )
    {
        IQueryable<AuditEntry> entries = context.Set<AuditEntry>().AsNoTracking();
        if (query.ActorId is { } actorId)
        {
            entries = entries.Where(entry => entry.ActorId == actorId);
        }

        if (query.From is { } from)
        {
            DateTimeOffset start = IstanbulCalendar.StartOfDay(from);
            entries = entries.Where(entry => entry.OccurredAt >= start);
        }

        if (query.To is { } to)
        {
            DateTimeOffset end = IstanbulCalendar.StartOfDay(to);
            entries = entries.Where(entry => entry.OccurredAt < end);
        }

        if (!string.IsNullOrWhiteSpace(query.Module))
        {
            entries = entries.Where(entry => entry.Module == query.Module);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            entries = entries.Where(entry => entry.EntityType == query.EntityType);
        }

        if (query.EntityId is { } entityId)
        {
            entries = entries.Where(entry => entry.EntityId == entityId);
        }

        if (query.Cursor.After is { } after)
        {
            if (!TimeAndIdCursor.TryDecode(after, out DateTimeOffset afterTime, out Guid afterId))
            {
                throw new ValidationFailedException([
                    new ValidationError(
                        "After",
                        InvalidCursorCode,
                        new Dictionary<string, object?>(StringComparer.Ordinal)
                    ),
                ]);
            }

            // A row value comparison, which the (occurred_at, id) index serves (audit AU-03).
            entries = entries.Where(entry =>
                EF.Functions.LessThan(
                    ValueTuple.Create(entry.OccurredAt, entry.Id),
                    ValueTuple.Create(afterTime, afterId)
                )
            );
        }

        List<AuditEntry> rows = await entries
            .OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id)
            .Take(query.Cursor.Limit + 1)
            .ToListAsync(cancellationToken);
        List<AuditEntryItem> items =
        [
            .. rows.Take(query.Cursor.Limit)
                .Select(entry => new AuditEntryItem(
                    entry.Id,
                    entry.OccurredAt,
                    entry.ActorId,
                    entry.ActorName,
                    entry.Module,
                    entry.EntityType,
                    entry.EntityId,
                    entry.Action,
                    JsonDocument.Parse(entry.Changes).RootElement.Clone(),
                    entry.TraceId
                )),
        ];
        string? next =
            rows.Count > query.Cursor.Limit ? TimeAndIdCursor.Encode(items[^1].OccurredAt, items[^1].Id) : null;
        return new CursorResult<AuditEntryItem>(items, next);
    }
}
