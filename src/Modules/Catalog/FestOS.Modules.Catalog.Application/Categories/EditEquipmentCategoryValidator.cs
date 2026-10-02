using FestOS.Modules.Catalog.Domain.Categories;
using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Categories;

internal sealed class EditEquipmentCategoryValidator : AbstractValidator<EditEquipmentCategoryCommand>
{
    public EditEquipmentCategoryValidator() =>
        RuleFor(command => command.Name).NotEmpty().MaximumLength(EquipmentCategory.NameMaxLength);
}
