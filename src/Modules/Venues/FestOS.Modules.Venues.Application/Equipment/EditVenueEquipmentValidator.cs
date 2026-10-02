using FluentValidation;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class EditVenueEquipmentValidator : AbstractValidator<EditVenueEquipmentCommand>
{
    public EditVenueEquipmentValidator() => this.ValidateLine(command => command.Details);
}
