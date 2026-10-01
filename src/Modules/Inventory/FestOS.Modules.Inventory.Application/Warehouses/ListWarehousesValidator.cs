using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class ListWarehousesValidator : AbstractValidator<ListWarehousesQuery>
{
    public ListWarehousesValidator()
    {
        RuleFor(query => query.Q).MaximumLength(ListWarehousesQuery.MaxSearchLength);
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Sort).SortableBy([.. ListWarehousesQuery.SortFields]);
        RuleFor(query => query.Page).SetValidator(new PageRequestValidator());
    }
}
