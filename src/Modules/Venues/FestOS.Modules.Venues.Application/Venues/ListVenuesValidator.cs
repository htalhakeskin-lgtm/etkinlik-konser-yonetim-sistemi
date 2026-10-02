using FestOS.BuildingBlocks.Application.Paging;
using FestOS.Modules.Venues.Domain.Venues;
using FluentValidation;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class ListVenuesValidator : AbstractValidator<ListVenuesQuery>
{
    public ListVenuesValidator()
    {
        RuleFor(query => query.Q).MaximumLength(ListVenuesQuery.MaxSearchLength);
        RuleFor(query => query.City).MaximumLength(Venue.CityMaxLength);
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Sort).SortableBy([.. ListVenuesQuery.SortFields]);
        RuleFor(query => query.Page).SetValidator(new PageRequestValidator());
    }
}
