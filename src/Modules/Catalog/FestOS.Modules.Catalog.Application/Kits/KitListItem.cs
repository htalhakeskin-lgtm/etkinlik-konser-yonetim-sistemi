using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>A row of the kits list; <c>Version</c> lets the row's actions send <c>If-Match</c> (api §9).</summary>
public sealed record KitListItem(KitId Id, string Name, int LineCount, KitTotals Totals, bool IsActive, int Version);
