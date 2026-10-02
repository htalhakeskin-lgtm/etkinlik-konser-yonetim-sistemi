using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class EditEquipmentModelValidator : AbstractValidator<EditEquipmentModelCommand>
{
    public EditEquipmentModelValidator() => this.ValidateModel();
}
