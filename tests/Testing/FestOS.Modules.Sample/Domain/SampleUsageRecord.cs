using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Sample.Domain;

/// <summary>Written by an integration event listener, so tests can see what it did.</summary>
public sealed class SampleUsageRecord : AggregateRoot<SampleUsageRecordId>
{
    private SampleUsageRecord(SampleUsageRecordId id)
        : base(id) { }

    /// <summary>The item that was used.</summary>
    public Guid SampleItemId { get; private set; }

    /// <summary>Records a use.</summary>
    public static SampleUsageRecord Create(Guid sampleItemId) =>
        new(SampleUsageRecordId.New()) { SampleItemId = sampleItemId };
}
