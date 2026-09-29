namespace FestOS.BuildingBlocks.Domain.Rules;

/// <summary>
/// The kinds of business rule that can be violated (docs/03-business-rules.md §3). Trigger and
/// calculation rules describe effects and results, so they are never thrown.
/// </summary>
public enum RuleKind
{
    /// <summary>A condition that must always hold; the API answers 422.</summary>
    Constraint,

    /// <summary>The condition of a status transition; the API answers 422.</summary>
    Transition,

    /// <summary>An access rule with a domain meaning; the API answers 403.</summary>
    Authorization,
}
