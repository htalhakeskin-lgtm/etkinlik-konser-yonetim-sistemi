using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class ListKitsValidator : AbstractValidator<ListKitsQuery>
{
    public ListKitsValidator()
    {
        RuleFor(query => query.Q).MaximumLength(ListKitsQuery.MaxSearchLength);
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Page).SetValidator(new PageRequestValidator());
    }
}
