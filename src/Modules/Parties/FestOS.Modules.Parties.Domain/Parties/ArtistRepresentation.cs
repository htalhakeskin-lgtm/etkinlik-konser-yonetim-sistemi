using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>
/// An agency representing an artist (06 decision 2, BR-PTY-004); part of the artist (06 §7). Several agencies
/// may represent one artist, e.g. in different regions (06 E-02).
/// </summary>
public sealed class ArtistRepresentation : Entity<ArtistRepresentationId>
{
    /// <summary>The longest description.</summary>
    public const int DescriptionMaxLength = 200;

    internal ArtistRepresentation(ArtistRepresentationId id, PartyId agencyId, string? description)
        : this(id)
    {
        AgencyId = agencyId;
        Describe(description);
    }

    private ArtistRepresentation(ArtistRepresentationId id)
        : base(id) { }

    /// <summary>The agency, a party with the agency role.</summary>
    public PartyId AgencyId { get; private set; }

    /// <summary>What the representation covers, e.g. a region.</summary>
    public string? Description { get; private set; }

    internal void Describe(string? description) =>
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
