using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Domain.Kits;

/// <summary>A model or another kit in a <see cref="Kit"/>, with a quantity; part of the kit (06 §7).</summary>
public sealed class KitLine : Entity<KitLineId>
{
    internal KitLine(KitLineId id, KitLineDetails details, int sortOrder)
        : this(id)
    {
        ModelId = details.ModelId;
        SubKitId = details.SubKitId;
        Change(details.Quantity, sortOrder);
    }

    private KitLine(KitLineId id)
        : base(id) { }

    /// <summary>The model, or empty when the line is a kit.</summary>
    public EquipmentModelId? ModelId { get; private set; }

    /// <summary>The kit, or empty when the line is a model.</summary>
    public KitId? SubKitId { get; private set; }

    /// <summary>How many.</summary>
    public int Quantity { get; private set; }

    /// <summary>Its place in the kit.</summary>
    public int SortOrder { get; private set; }

    internal bool Targets(KitLineDetails details) => ModelId == details.ModelId && SubKitId == details.SubKitId;

    internal void Change(int quantity, int sortOrder)
    {
        Quantity = quantity;
        SortOrder = sortOrder;
    }
}
