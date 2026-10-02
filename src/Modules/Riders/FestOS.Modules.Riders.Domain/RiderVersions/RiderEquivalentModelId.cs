using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>Identifies a <see cref="RiderEquivalentModel"/>.</summary>
public readonly record struct RiderEquivalentModelId(Guid Value) : IStronglyTypedId<RiderEquivalentModelId>
{
    /// <inheritdoc />
    public static RiderEquivalentModelId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static RiderEquivalentModelId New() => new(Guid.CreateVersion7());
}
