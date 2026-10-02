using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>A model that may replace a flexible line's model, in order of preference (BR-RDR-002).</summary>
[NotAudited]
public sealed class RiderEquivalentModel : Entity<RiderEquivalentModelId>
{
    internal RiderEquivalentModel(RiderEquivalentModelId id, Guid modelId, int sortOrder)
        : this(id)
    {
        ModelId = modelId;
        SortOrder = sortOrder;
    }

    private RiderEquivalentModel(RiderEquivalentModelId id)
        : base(id) { }

    /// <summary>The model; another module's record, by identifier.</summary>
    public Guid ModelId { get; private set; }

    /// <summary>Its place among the line's equivalents.</summary>
    public int SortOrder { get; private set; }
}
