using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>Identifies a <see cref="RiderLine"/>.</summary>
public readonly record struct RiderLineId(Guid Value) : IStronglyTypedId<RiderLineId>
{
    /// <inheritdoc />
    public static RiderLineId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static RiderLineId New() => new(Guid.CreateVersion7());
}
