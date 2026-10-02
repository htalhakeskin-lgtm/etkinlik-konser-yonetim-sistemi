namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>
/// A contact point as a form sends it. <paramref name="Id"/> names a contact point the party already has,
/// so the change history shows it changed rather than removed and added again (parties §8).
/// </summary>
/// <param name="Id">The existing contact point, or <see langword="null"/> for a new one.</param>
/// <param name="Kind">Phone, e-mail or address.</param>
/// <param name="Value">The number, address or e-mail.</param>
/// <param name="Label">An optional label, e.g. "Muhasebe".</param>
/// <param name="IsPrimary">Whether it is the primary of its kind.</param>
public sealed record ContactPointDetails(
    ContactPointId? Id,
    ContactPointKind Kind,
    string Value,
    string? Label,
    bool IsPrimary
);
