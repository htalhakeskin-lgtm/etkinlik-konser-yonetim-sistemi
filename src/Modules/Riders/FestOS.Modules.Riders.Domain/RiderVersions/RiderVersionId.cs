using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>Identifies a <see cref="RiderVersion"/>.</summary>
public readonly record struct RiderVersionId(Guid Value) : IStronglyTypedId<RiderVersionId>
{
    /// <inheritdoc />
    public static RiderVersionId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static RiderVersionId New() => new(Guid.CreateVersion7());
}
