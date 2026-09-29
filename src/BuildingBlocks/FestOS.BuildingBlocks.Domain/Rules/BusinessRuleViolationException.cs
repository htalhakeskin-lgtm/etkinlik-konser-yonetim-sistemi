using System.Collections.ObjectModel;

namespace FestOS.BuildingBlocks.Domain.Rules;

/// <summary>
/// A business rule rejected the operation. The API returns the rule code in the problem's
/// <c>code</c> field and the parameters in <c>params</c>, so the front end can show the Turkish
/// message for that rule (docs/standards/api.md §8).
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="ruleCode">The rule number, taken from the module's <c>…RuleCodes</c> constants.</param>
    /// <param name="message">An English explanation for developers; no personal or secret data.</param>
    /// <param name="kind">The kind of the violated rule; decides between 422 and 403.</param>
    /// <param name="parameters">Values the user-facing message needs.</param>
    public BusinessRuleViolationException(
        string ruleCode,
        string message,
        RuleKind kind = RuleKind.Constraint,
        IReadOnlyDictionary<string, object?>? parameters = null
    )
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleCode);
        RuleCode = ruleCode;
        Kind = kind;
        Parameters = parameters ?? ReadOnlyDictionary<string, object?>.Empty;
    }

    /// <summary>The rule number, in the form <c>BR-{module}-{number}</c>.</summary>
    public string RuleCode { get; }

    /// <summary>The kind of the violated rule.</summary>
    public RuleKind Kind { get; }

    /// <summary>Values the user-facing message needs, e.g. who performed a conflicting action.</summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; }
}
