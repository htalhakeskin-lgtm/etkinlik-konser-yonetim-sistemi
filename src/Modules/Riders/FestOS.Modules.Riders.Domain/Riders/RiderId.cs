using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Riders.Domain.Riders;

/// <summary>Identifies a <see cref="Rider"/>.</summary>
public readonly record struct RiderId(Guid Value) : IStronglyTypedId<RiderId>
{
    /// <inheritdoc />
    public static RiderId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static RiderId New() => new(Guid.CreateVersion7());
}
