using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>Identifies a <see cref="Party"/>.</summary>
public readonly record struct PartyId(Guid Value) : IStronglyTypedId<PartyId>
{
    /// <inheritdoc />
    public static PartyId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static PartyId New() => new(Guid.CreateVersion7());
}
