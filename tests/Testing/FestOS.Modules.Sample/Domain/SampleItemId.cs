using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Sample.Domain;

/// <summary>Identifies a <see cref="SampleItem"/>.</summary>
public readonly record struct SampleItemId(Guid Value) : IStronglyTypedId<SampleItemId>
{
    /// <inheritdoc />
    public static SampleItemId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static SampleItemId New() => new(Guid.CreateVersion7());
}
