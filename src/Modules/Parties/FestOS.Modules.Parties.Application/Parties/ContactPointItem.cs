using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>A contact point of a party, in its order; the edit dialog sends <see cref="Id"/> back.</summary>
public sealed record ContactPointItem(
    ContactPointId Id,
    ContactPointKind Kind,
    string Value,
    string? Label,
    bool IsPrimary
);
