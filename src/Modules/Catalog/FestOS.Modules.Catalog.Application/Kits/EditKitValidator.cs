using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class EditKitValidator : AbstractValidator<EditKitCommand>
{
    public EditKitValidator() => this.ValidateKit();
}
