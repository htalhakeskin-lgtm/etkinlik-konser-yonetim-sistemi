using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.Modules.Catalog.Domain.Kits;

/// <summary>
/// Equipment often used together, e.g. a small stage light pack (US-EQP-005): lines of models and other
/// kits. Riders refer to it, so it is never deleted, only deactivated (BR-SYS-001). Whether a sub-kit leads
/// back to this one spans several kits, so the command checks it under a lock (catalog CT-03).
/// </summary>
public sealed class Kit : AggregateRoot<KitId>, IDeactivatable
{
    /// <summary>The longest name.</summary>
    public const int NameMaxLength = 200;

    /// <summary>The most lines a kit keeps.</summary>
    public const int MaxLines = 200;

    private readonly List<KitLine> _lines = [];

    private Kit(KitId id)
        : base(id) { }

    /// <summary>The name, unique among all kits (BR-EQP-013).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The name's search key (database §13).</summary>
    [NotAudited]
    public string NameSearch { get; private set; } = string.Empty;

    /// <summary>The lines in their order.</summary>
    public IReadOnlyList<KitLine> Lines => _lines;

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>A new, active kit.</summary>
    public static Kit Create(string name, IReadOnlyList<KitLineDetails> lines)
    {
        var kit = new Kit(KitId.New());
        kit.Edit(name, lines);
        return kit;
    }

    /// <summary>
    /// Renames the kit and sets its lines; a line for the same model or kit as before keeps its identity, so
    /// the change history shows it changed (catalog CT-04). A kit cannot list itself (BR-EQP-003).
    /// </summary>
    public void Edit(string name, IReadOnlyList<KitLineDetails> lines)
    {
        if (lines.Any(line => line.SubKitId == Id))
        {
            throw new BusinessRuleViolationException(CatalogRuleCodes.KitStructure, "A kit cannot hold itself.");
        }

        Name = name.Trim();
        NameSearch = SearchKey.Of(name);
        _lines.RemoveAll(existing => !lines.Any(existing.Targets));
        for (int index = 0; index < lines.Count; index++)
        {
            KitLineDetails details = lines[index];
            KitLine? existing = _lines.Find(line => line.Targets(details));
            if (existing is null)
            {
                _lines.Add(new KitLine(KitLineId.New(), details, index));
            }
            else
            {
                existing.Change(details.Quantity, index);
            }
        }

        _lines.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
    }

    /// <summary>Takes the kit out of new selections (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens a deactivated kit again.</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }
}
