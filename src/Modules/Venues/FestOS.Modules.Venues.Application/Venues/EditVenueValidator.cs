using FluentValidation;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class EditVenueValidator : AbstractValidator<EditVenueCommand>
{
    public EditVenueValidator() => this.ValidateVenue();
}
