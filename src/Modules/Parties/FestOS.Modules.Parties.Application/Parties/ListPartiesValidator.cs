using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class ListPartiesValidator : AbstractValidator<ListPartiesQuery>
{
    public ListPartiesValidator()
    {
        RuleFor(query => query.Q).MaximumLength(ListPartiesQuery.MaxSearchLength);
        RuleFor(query => query.Role).IsInEnum();
        RuleFor(query => query.Kind).IsInEnum();
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Sort).SortableBy([.. ListPartiesQuery.SortFields]);
        RuleFor(query => query.Page).SetValidator(new PageRequestValidator());
    }
}
