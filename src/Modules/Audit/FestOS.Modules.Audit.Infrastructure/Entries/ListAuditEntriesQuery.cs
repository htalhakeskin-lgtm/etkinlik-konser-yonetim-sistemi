using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.Modules.Audit.Infrastructure.Entries;

/// <summary>
/// The change history from newest to oldest, one slice at a time (US-SYS-004, audit AU-03). A record's
/// history is the same list filtered by its root's type and id, so the changes of its parts show too (AU-02,
/// parties MD-01). The days are Istanbul days, from
/// <paramref name="From"/> up to, not including, <paramref name="To"/> (api §6.3).
/// </summary>
public sealed record ListAuditEntriesQuery(
    Guid? ActorId,
    DateOnly? From,
    DateOnly? To,
    string? Module,
    string? RootType,
    Guid? RootId,
    CursorRequest Cursor
) : IQuery<CursorResult<AuditEntryItem>>
{
    /// <summary>The longest module or record type name.</summary>
    public const int MaxNameLength = 200;
}
