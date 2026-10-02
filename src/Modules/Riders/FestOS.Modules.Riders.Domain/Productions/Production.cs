using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.Modules.Riders.Domain.Productions;

/// <summary>
/// A show an artist tours with, the owner of the production rider (US-ART-001). Events refer to it, so it is
/// never deleted, only deactivated (BR-SYS-001); its artist never changes (riders §5).
/// </summary>
public sealed class Production : AggregateRoot<ProductionId>, IDeactivatable
{
    /// <summary>The longest name.</summary>
    public const int NameMaxLength = 200;

    /// <summary>The longest description.</summary>
    public const int DescriptionMaxLength = 2000;

    private Production(ProductionId id)
        : base(id) { }

    /// <summary>The artist; another module's record, by identifier (05 §2, principle 7).</summary>
    public Guid ArtistPartyId { get; private set; }

    /// <summary>The name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The name's search key; with the artist, what makes two productions the same (BR-RDR-009).</summary>
    [NotAudited]
    public string NameSearch { get; private set; } = string.Empty;

    /// <summary>What the show is, in a few words.</summary>
    public string? Description { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>A new, active production; the command checks the artist (BR-PTY-004).</summary>
    public static Production Create(Guid artistPartyId, string name, string? description)
    {
        var production = new Production(ProductionId.New()) { ArtistPartyId = artistPartyId };
        production.Edit(name, description);
        return production;
    }

    /// <summary>Changes the name and the description.</summary>
    public void Edit(string name, string? description)
    {
        Name = name.Trim();
        NameSearch = SearchKey.Of(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    /// <summary>Takes the production out of new selections (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens a deactivated production again.</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }
}
