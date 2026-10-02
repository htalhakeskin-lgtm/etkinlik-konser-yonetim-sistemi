using FestOS.Modules.Parties.Domain.Parties;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class RetitleContactPersonValidator : AbstractValidator<RetitleContactPersonCommand>
{
    public RetitleContactPersonValidator() =>
        RuleFor(command => command.Title).MaximumLength(OrganizationContact.TitleMaxLength);
}
