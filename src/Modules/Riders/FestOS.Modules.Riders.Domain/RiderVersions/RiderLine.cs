using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>One need of a rider version: a model or a category, how many, and how strictly (US-RDR-001).</summary>
[NotAudited]
public sealed class RiderLine : Entity<RiderLineId>
{
    /// <summary>The most a line asks for.</summary>
    public const int MaxQuantity = 9999;

    /// <summary>The longest note.</summary>
    public const int NoteMaxLength = 2000;

    /// <summary>The most equivalents a line has.</summary>
    public const int MaxEquivalents = 10;

    private readonly List<RiderEquivalentModel> _equivalents = [];

    internal RiderLine(RiderLineId id, RiderLineDetails details, int sortOrder)
        : this(id)
    {
        LineKey = details.LineKey ?? Guid.CreateVersion7();
        SortOrder = sortOrder;
        ModelId = details.ModelId;
        CategoryId = details.CategoryId;
        Quantity = details.Quantity;
        Flexibility = details.Flexibility;
        Note = string.IsNullOrWhiteSpace(details.Note) ? null : details.Note.Trim();
        _equivalents.AddRange(
            details.EquivalentModelIds.Select(
                (modelId, index) => new RiderEquivalentModel(RiderEquivalentModelId.New(), modelId, index)
            )
        );
    }

    private RiderLine(RiderLineId id)
        : base(id) { }

    /// <summary>The line's identity across versions (riders RD-03).</summary>
    public Guid LineKey { get; private set; }

    /// <summary>Its place in the version.</summary>
    public int SortOrder { get; private set; }

    /// <summary>The model, when the line asks for one; another module's record.</summary>
    public Guid? ModelId { get; private set; }

    /// <summary>The category, when any of its models will do; another module's record.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>A kit, on a technical service requirement list only (1.4).</summary>
    public Guid? KitId { get; private set; }

    /// <summary>How many.</summary>
    public int Quantity { get; private set; }

    /// <summary>On a model line, whether equivalents will do.</summary>
    public RiderLineFlexibility? Flexibility { get; private set; }

    /// <summary>What the artist adds.</summary>
    public string? Note { get; private set; }

    /// <summary>The equivalents, in order of preference.</summary>
    public IReadOnlyList<RiderEquivalentModel> Equivalents => _equivalents;
}
