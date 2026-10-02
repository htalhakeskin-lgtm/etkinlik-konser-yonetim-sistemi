using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>A contact point in a party's body; <see cref="Id"/> names one the party already has.</summary>
public sealed record ContactPointRequest(
    ContactPointKind Kind,
    string Value,
    bool IsPrimary,
    Guid? Id = null,
    string? Label = null
);
