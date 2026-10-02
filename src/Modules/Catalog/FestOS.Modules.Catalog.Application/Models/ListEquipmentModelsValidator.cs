using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class ListEquipmentModelsValidator : AbstractValidator<ListEquipmentModelsQuery>
{
    public ListEquipmentModelsValidator()
    {
        RuleFor(query => query.Q).MaximumLength(ListEquipmentModelsQuery.MaxSearchLength);
        RuleFor(query => query.TrackingType).IsInEnum();
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Sort).SortableBy([.. ListEquipmentModelsQuery.SortFields]);
        RuleFor(query => query.Page).SetValidator(new PageRequestValidator());
    }
}
