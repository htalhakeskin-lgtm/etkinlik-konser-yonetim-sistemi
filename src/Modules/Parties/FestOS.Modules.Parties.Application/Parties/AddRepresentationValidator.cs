using FestOS.Modules.Parties.Domain.Parties;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class AddRepresentationValidator : AbstractValidator<AddRepresentationCommand>
{
    public AddRepresentationValidator() =>
        RuleFor(command => command.Description).MaximumLength(ArtistRepresentation.DescriptionMaxLength);
}
