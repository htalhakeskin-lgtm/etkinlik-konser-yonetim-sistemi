using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>A kit as its page shows it; <see cref="Version"/> is also the <c>ETag</c> (api §9).</summary>
public sealed record KitDetails(
    KitId Id,
    string Name,
    IReadOnlyList<KitLineItem> Lines,
    IReadOnlyList<KitContentItem> Contents,
    KitTotals Totals,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
