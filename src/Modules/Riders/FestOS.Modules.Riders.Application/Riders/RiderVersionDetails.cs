using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>A version as the rider view shows it; it never changes, so it can be cached for long.</summary>
public sealed record RiderVersionDetails(
    RiderVersionId Id,
    RiderId RiderId,
    int Number,
    string? Note,
    DateTimeOffset CreatedAt,
    string CreatedByName,
    IReadOnlyList<RiderLineItem> Lines
);
