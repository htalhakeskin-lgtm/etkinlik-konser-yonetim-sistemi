using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>A row of the versions list.</summary>
public sealed record RiderVersionListItem(
    RiderVersionId Id,
    int Number,
    string? Note,
    DateTimeOffset CreatedAt,
    string CreatedByName,
    int LineCount
);
