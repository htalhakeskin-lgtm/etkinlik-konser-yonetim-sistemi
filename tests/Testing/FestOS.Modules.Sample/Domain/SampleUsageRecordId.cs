using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Sample.Domain;

/// <summary>Identifies a <see cref="SampleUsageRecord"/>.</summary>
public readonly record struct SampleUsageRecordId(Guid Value) : IStronglyTypedId<SampleUsageRecordId>
{
    /// <inheritdoc />
    public static SampleUsageRecordId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static SampleUsageRecordId New() => new(Guid.CreateVersion7());
}
