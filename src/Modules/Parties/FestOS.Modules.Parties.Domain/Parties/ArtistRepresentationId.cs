using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>Identifies an <see cref="ArtistRepresentation"/>.</summary>
public readonly record struct ArtistRepresentationId(Guid Value) : IStronglyTypedId<ArtistRepresentationId>
{
    /// <inheritdoc />
    public static ArtistRepresentationId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static ArtistRepresentationId New() => new(Guid.CreateVersion7());
}
