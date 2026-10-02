using FestOS.Modules.Catalog.Application.Kits;
using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

/// <summary>
/// Every kit's lines and the models' values, read once to open kits down to models and sum them
/// (BR-EQP-003). The catalog is small; a kit list page reads it in two queries.
/// </summary>
internal sealed class KitCatalog
{
    private readonly Dictionary<KitId, KitRow> _kits;
    private readonly Dictionary<EquipmentModelId, ModelRow> _models;

    private KitCatalog(Dictionary<KitId, KitRow> kits, Dictionary<EquipmentModelId, ModelRow> models)
    {
        _kits = kits;
        _models = models;
    }

    public static async Task<KitCatalog> ReadAsync(CatalogDbContext context, CancellationToken cancellationToken)
    {
        List<KitRow> kits = await context
            .Kits.AsNoTracking()
            .Select(kit => new KitRow(
                kit.Id,
                kit.Name,
                kit.DeactivatedAt == null,
                kit.Lines.OrderBy(line => line.SortOrder)
                    .Select(line => new LineRow(line.Id, line.ModelId, line.SubKitId, line.Quantity))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
        List<ModelRow> models = await context
            .Models.AsNoTracking()
            .Select(model => new ModelRow(
                model.Id,
                model.Brand + " " + model.Name,
                model.WeightKilograms,
                model.PowerWatts,
                model.DeactivatedAt == null
            ))
            .ToListAsync(cancellationToken);
        return new KitCatalog(kits.ToDictionary(kit => kit.Id), models.ToDictionary(model => model.Id));
    }

    public KitTotals TotalsOf(KitId id)
    {
        IReadOnlyList<KitContentItem> contents = ContentsOf(id);
        decimal weight = 0m;
        int power = 0;
        bool isWeightComplete = true;
        bool isPowerComplete = true;
        foreach (KitContentItem item in contents)
        {
            ModelRow model = _models[item.ModelId];
            isWeightComplete &= model.WeightKilograms is not null;
            isPowerComplete &= model.PowerWatts is not null;
            weight += (model.WeightKilograms ?? 0m) * item.Quantity;
            power += (model.PowerWatts ?? 0) * item.Quantity;
        }

        return new KitTotals(weight, isWeightComplete, power, isPowerComplete);
    }

    /// <summary>The kit opened down to models, each with its total quantity, in the order first met.</summary>
    public IReadOnlyList<KitContentItem> ContentsOf(KitId id)
    {
        Dictionary<EquipmentModelId, int> quantities = [];
        List<EquipmentModelId> order = [];
        Open(id, 1, quantities, order, []);
        return [.. order.Select(model => new KitContentItem(model, _models[model].Name, quantities[model]))];
    }

    public IReadOnlyList<KitLineItem> LinesOf(KitId id) =>
        [
            .. _kits[id]
                .Lines.Select(line =>
                    line.ModelId is { } model
                        ? new KitLineItem(
                            line.Id,
                            model,
                            null,
                            _models[model].Name,
                            line.Quantity,
                            _models[model].IsActive
                        )
                        : new KitLineItem(
                            line.Id,
                            null,
                            line.SubKitId,
                            _kits[line.SubKitId!.Value].Name,
                            line.Quantity,
                            _kits[line.SubKitId!.Value].IsActive
                        )
                ),
        ];

    // The command refuses cycles (BR-EQP-003); the visited set only guards against a damaged structure.
    private void Open(
        KitId id,
        int multiplier,
        Dictionary<EquipmentModelId, int> quantities,
        List<EquipmentModelId> order,
        HashSet<KitId> path
    )
    {
        if (!path.Add(id) || !_kits.TryGetValue(id, out KitRow? kit))
        {
            return;
        }

        foreach (LineRow line in kit.Lines)
        {
            if (line.ModelId is { } model)
            {
                if (!quantities.ContainsKey(model))
                {
                    order.Add(model);
                }

                quantities[model] = quantities.GetValueOrDefault(model) + (line.Quantity * multiplier);
            }
            else if (line.SubKitId is { } subKit)
            {
                Open(subKit, line.Quantity * multiplier, quantities, order, path);
            }
        }

        path.Remove(id);
    }

    private sealed record KitRow(KitId Id, string Name, bool IsActive, List<LineRow> Lines);

    private sealed record LineRow(KitLineId Id, EquipmentModelId? ModelId, KitId? SubKitId, int Quantity);

    private sealed record ModelRow(
        EquipmentModelId Id,
        string Name,
        decimal? WeightKilograms,
        int? PowerWatts,
        bool IsActive
    );
}
