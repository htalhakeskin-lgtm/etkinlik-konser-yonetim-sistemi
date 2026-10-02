using FluentValidation;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class AddVenueEquipmentValidator : AbstractValidator<AddVenueEquipmentCommand>
{
    public AddVenueEquipmentValidator() => this.ValidateLine(command => command.Details);
}
