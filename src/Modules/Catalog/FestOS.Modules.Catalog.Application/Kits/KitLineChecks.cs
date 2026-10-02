using System.Globalization;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Catalog.Domain;
using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>
/// The checks of a kit's lines that need other records (BR-SYS-001, BR-EQP-003): every target exists, a
/// new line takes only active models and kits, and no sub-kit leads back to the kit.
/// </summary>
internal static class KitLineChecks
{
    public static async Task EnsureLinesFitAsync(
        this IKitRepository kits,
        KitId kitId,
        IReadOnlyList<KitLineDetails> lines,
        IReadOnlyList<KitLine> keptLines,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<KitId, IReadOnlyList<KitId>> structure = await kits.LockStructureAsync(cancellationToken);
        IReadOnlyDictionary<EquipmentModelId, bool> models = await kits.FindModelsAsync(
            [.. lines.Select(line => line.ModelId).OfType<EquipmentModelId>()],
            cancellationToken
        );
        IReadOnlyDictionary<KitId, bool> subKits = await kits.FindKitsAsync(
            [.. lines.Select(line => line.SubKitId).OfType<KitId>()],
            cancellationToken
        );

        List<ValidationError> errors = [];
        for (int index = 0; index < lines.Count; index++)
        {
            KitLineDetails line = lines[index];
            bool exists = line.ModelId is { } model
                ? models.ContainsKey(model)
                : subKits.ContainsKey(line.SubKitId!.Value);
            bool isActive = line.ModelId is { } activeModel
                ? models.GetValueOrDefault(activeModel)
                : subKits.GetValueOrDefault(line.SubKitId!.Value);
            bool wasThere = keptLines.Any(kept => kept.ModelId == line.ModelId && kept.SubKitId == line.SubKitId);
            if (!exists || (!isActive && !wasThere))
            {
                errors.Add(
                    new ValidationError(
                        string.Create(CultureInfo.InvariantCulture, $"Lines[{index}]"),
                        "invalidValue",
                        new Dictionary<string, object?>(StringComparer.Ordinal)
                    )
                );
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationFailedException(errors);
        }

        foreach (KitId subKit in lines.Select(line => line.SubKitId).OfType<KitId>())
        {
            if (Reaches(structure, subKit, kitId))
            {
                throw new BusinessRuleViolationException(
                    CatalogRuleCodes.KitStructure,
                    "A kit cannot hold itself through another kit."
                );
            }
        }
    }

    // Whether the kits that start holds lead, through any depth, to target.
    private static bool Reaches(IReadOnlyDictionary<KitId, IReadOnlyList<KitId>> structure, KitId start, KitId target)
    {
        HashSet<KitId> seen = [];
        Stack<KitId> next = new([start]);
        while (next.TryPop(out KitId current))
        {
            if (current == target)
            {
                return true;
            }

            if (seen.Add(current) && structure.TryGetValue(current, out IReadOnlyList<KitId>? held))
            {
                foreach (KitId kit in held)
                {
                    next.Push(kit);
                }
            }
        }

        return false;
    }
}
