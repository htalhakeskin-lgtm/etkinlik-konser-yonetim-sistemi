using FluentValidation;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class CreateProductionValidator : AbstractValidator<CreateProductionCommand>
{
    public CreateProductionValidator()
    {
        RuleFor(command => command.ArtistPartyId).NotEmpty();
        this.ValidateProduction();
    }
}
