using FluentValidation;

namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>FluentValidation rule for the <c>sort</c> parameter of list queries.</summary>
public static class SortRuleExtensions
{
    /// <summary>
    /// Allows only the listed fields (api §6.2): user input never reaches SQL as a column name. The
    /// allowed fields travel with the error so the message can list them.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> SortableBy<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        params string[] allowedFields
    )
    {
        ArgumentNullException.ThrowIfNull(ruleBuilder);

        return ruleBuilder
            .Must(
                (_, text, context) =>
                {
                    context.MessageFormatter.AppendArgument("AllowedFields", string.Join(',', allowedFields));
                    return SortSpec.IsValid(text, allowedFields);
                }
            )
            .WithErrorCode(SortSpec.UnsupportedFieldErrorCode);
    }
}
