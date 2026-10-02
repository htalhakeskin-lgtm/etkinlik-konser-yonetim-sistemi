using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class CreateKitValidator : AbstractValidator<CreateKitCommand>
{
    public CreateKitValidator() => this.ValidateKit();
}
