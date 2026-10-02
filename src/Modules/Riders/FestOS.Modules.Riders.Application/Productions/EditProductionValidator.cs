using FluentValidation;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class EditProductionValidator : AbstractValidator<EditProductionCommand>
{
    public EditProductionValidator() => this.ValidateProduction();
}
