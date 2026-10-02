using FestOS.Modules.Riders.Domain.Productions;
using FluentValidation;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>The field checks a production gets when created or edited (riders §6).</summary>
internal static class ProductionValidation
{
    public static void ValidateProduction<T>(this AbstractValidator<T> validator)
        where T : IProductionCommand
    {
        validator.RuleFor(command => command.Name).NotEmpty().MaximumLength(Production.NameMaxLength);
        validator.RuleFor(command => command.Description).MaximumLength(Production.DescriptionMaxLength);
    }
}
