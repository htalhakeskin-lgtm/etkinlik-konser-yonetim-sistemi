using System.Globalization;
using FestOS.BuildingBlocks.Domain.Rules;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>The rules a version's lines keep (BR-RDR-001, BR-RDR-002); the refusal names the line and why.</summary>
public static class RiderLineRules
{
    /// <summary>Refuses the first line that breaks a rule.</summary>
    public static void EnsureValid(IReadOnlyList<RiderLineDetails> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        for (int index = 0; index < lines.Count; index++)
        {
            RiderLineDetails line = lines[index];

            // One target: a model or a category; a kit only on a requirement list (1.4).
            if (line.ModelId.HasValue == line.CategoryId.HasValue)
            {
                throw Refused(RidersRuleCodes.LineTarget, index, "target");
            }

            if (line.Quantity < 1)
            {
                throw Refused(RidersRuleCodes.LineTarget, index, "quantity");
            }

            // Any of a category's models will do, so flexibility belongs to model lines (riders RD-02).
            if (line.ModelId.HasValue != line.Flexibility.HasValue)
            {
                throw Refused(RidersRuleCodes.LineTarget, index, "flexibility");
            }

            if (line.EquivalentModelIds.Count == 0)
            {
                continue;
            }

            if (line.Flexibility != RiderLineFlexibility.Flexible)
            {
                throw Refused(RidersRuleCodes.EquivalentModels, index, "notFlexible");
            }

            if (line.EquivalentModelIds.Contains(line.ModelId!.Value))
            {
                throw Refused(RidersRuleCodes.EquivalentModels, index, "sameModel");
            }

            if (line.EquivalentModelIds.Distinct().Count() != line.EquivalentModelIds.Count)
            {
                throw Refused(RidersRuleCodes.EquivalentModels, index, "repeated");
            }
        }
    }

    private static BusinessRuleViolationException Refused(string rule, int index, string reason) =>
        new(
            rule,
            string.Create(CultureInfo.InvariantCulture, $"Rider line {index + 1} breaks {rule}: {reason}."),
            parameters: new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["line"] = index,
                ["reason"] = reason,
            }
        );
}
