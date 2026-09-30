using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Sample.Domain;

/// <summary>An aggregate that exercises the shared model rules: typed identifier, enum, amount and child entities.</summary>
public sealed class SampleItem : AggregateRoot<SampleItemId>
{
    private readonly List<SampleItemPart> _parts = [];

    private SampleItem(SampleItemId id)
        : base(id) { }

    /// <summary>The name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The status.</summary>
    public SampleItemStatus Status { get; private set; }

    /// <summary>An amount with more precision than kuruş.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>The parts; deleted with the item.</summary>
    public IReadOnlyList<SampleItemPart> Parts => _parts;

    /// <summary>Creates a draft item.</summary>
    public static SampleItem Create(string name, decimal unitPrice) =>
        new(SampleItemId.New()) { Name = name, UnitPrice = unitPrice };

    /// <summary>Adds a part, optionally replacing an earlier one.</summary>
    public SampleItemPart AddPart(string label, SampleItemPartId? replacesPartId = null)
    {
        var part = new SampleItemPart(SampleItemPartId.New(), label, replacesPartId);
        _parts.Add(part);
        return part;
    }

    /// <summary>Puts the item in use.</summary>
    public void Use()
    {
        Status = SampleItemStatus.InUse;
        Raise(new SampleItemUsedDomainEvent(Id));
    }

    /// <summary>Raises an event whose handler calls this again.</summary>
    public void Echo() => Raise(new SampleItemEchoedDomainEvent(Id));
}
