using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class ListProductionsValidator : AbstractValidator<ListProductionsQuery>
{
    public ListProductionsValidator()
    {
        RuleFor(query => query.Q).MaximumLength(ListProductionsQuery.MaxSearchLength);
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Sort).SortableBy([.. ListProductionsQuery.SortFields]);
        RuleFor(query => query.Page).SetValidator(new PageRequestValidator());
    }
}
