using FestOS.Modules.Catalog.Domain.Categories;
using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Categories;

internal sealed class CreateEquipmentCategoryValidator : AbstractValidator<CreateEquipmentCategoryCommand>
{
    public CreateEquipmentCategoryValidator() =>
        RuleFor(command => command.Name).NotEmpty().MaximumLength(EquipmentCategory.NameMaxLength);
}
