using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Sample.Domain;

/// <summary>Identifies a <see cref="SampleItemPart"/>.</summary>
public readonly record struct SampleItemPartId(Guid Value) : IStronglyTypedId<SampleItemPartId>
{
    /// <inheritdoc />
    public static SampleItemPartId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static SampleItemPartId New() => new(Guid.CreateVersion7());
}
