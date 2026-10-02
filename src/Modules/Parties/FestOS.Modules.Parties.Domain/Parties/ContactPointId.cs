using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>Identifies a <see cref="ContactPoint"/>.</summary>
public readonly record struct ContactPointId(Guid Value) : IStronglyTypedId<ContactPointId>
{
    /// <inheritdoc />
    public static ContactPointId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static ContactPointId New() => new(Guid.CreateVersion7());
}
