using FestOS.Modules.Catalog.Domain.Kits;
using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>The field checks a kit gets when created or edited (catalog §6).</summary>
internal static class KitValidation
{
    /// <summary>The largest quantity of a line.</summary>
    public const int MaxQuantity = 9999;

    public static void ValidateKit<T>(this AbstractValidator<T> validator)
        where T : IKitDescription
    {
        validator.RuleFor(kit => kit.Name).NotEmpty().MaximumLength(Kit.NameMaxLength);
        validator.RuleFor(kit => kit.Lines).NotNull().Must(lines => lines.Count <= Kit.MaxLines);
        validator
            .RuleForEach(kit => kit.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(details => details.Quantity).InclusiveBetween(1, MaxQuantity);
                line.RuleFor(details => details)
                    .Must(details => (details.ModelId is null) != (details.SubKitId is null))
                    .WithErrorCode("invalidValue")
                    .OverridePropertyName("Target");
            });

        // The same model or kit twice in one kit is one line with a larger quantity (catalog §5).
        validator
            .RuleFor(kit => kit.Lines)
            .Must(lines => lines.Select(line => (line.ModelId, line.SubKitId)).Distinct().Count() == lines.Count)
            .WithErrorCode("duplicateLine");
    }
}
