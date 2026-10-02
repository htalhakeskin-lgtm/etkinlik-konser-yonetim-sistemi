using FluentValidation;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class CreateVenueValidator : AbstractValidator<CreateVenueCommand>
{
    public CreateVenueValidator() => this.ValidateVenue();
}
