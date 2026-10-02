using FestOS.Modules.Parties.Domain.Parties;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class DescribeRepresentationValidator : AbstractValidator<DescribeRepresentationCommand>
{
    public DescribeRepresentationValidator() =>
        RuleFor(command => command.Description).MaximumLength(ArtistRepresentation.DescriptionMaxLength);
}
