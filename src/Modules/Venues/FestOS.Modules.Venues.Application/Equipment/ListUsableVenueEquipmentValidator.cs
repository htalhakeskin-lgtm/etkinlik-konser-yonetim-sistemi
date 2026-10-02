using FluentValidation;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class ListUsableVenueEquipmentValidator : AbstractValidator<ListUsableVenueEquipmentQuery>
{
    public ListUsableVenueEquipmentValidator() => RuleFor(query => query.To).GreaterThan(query => query.From);
}
