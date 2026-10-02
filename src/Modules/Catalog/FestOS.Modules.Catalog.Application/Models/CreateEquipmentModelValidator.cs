using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class CreateEquipmentModelValidator : AbstractValidator<CreateEquipmentModelCommand>
{
    public CreateEquipmentModelValidator() => this.ValidateModel();
}
