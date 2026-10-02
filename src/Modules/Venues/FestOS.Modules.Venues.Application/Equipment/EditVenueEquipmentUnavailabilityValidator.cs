using FluentValidation;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class EditVenueEquipmentUnavailabilityValidator
    : AbstractValidator<EditVenueEquipmentUnavailabilityCommand>
{
    public EditVenueEquipmentUnavailabilityValidator() => this.ValidatePeriod(command => command.Details);
}
