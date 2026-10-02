using FluentValidation;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class AddVenueEquipmentUnavailabilityValidator
    : AbstractValidator<AddVenueEquipmentUnavailabilityCommand>
{
    public AddVenueEquipmentUnavailabilityValidator() => this.ValidatePeriod(command => command.Details);
}
