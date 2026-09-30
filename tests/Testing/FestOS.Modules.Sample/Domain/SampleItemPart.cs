using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Sample.Domain;

/// <summary>A child entity of <see cref="SampleItem"/>.</summary>
public sealed class SampleItemPart : Entity<SampleItemPartId>
{
    internal SampleItemPart(SampleItemPartId id, string label, SampleItemPartId? replacesPartId)
        : base(id)
    {
        Label = label;
        ReplacesPartId = replacesPartId;
    }

    /// <summary>The label.</summary>
    public string Label { get; private set; }

    /// <summary>The part this one replaces; a reference whose delete behaviour is left to the conventions.</summary>
    public SampleItemPartId? ReplacesPartId { get; private set; }
}
