using FestOS.Modules.Parties.Domain.Parties;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class AddContactPersonValidator : AbstractValidator<AddContactPersonCommand>
{
    public AddContactPersonValidator() =>
        RuleFor(command => command.Title).MaximumLength(OrganizationContact.TitleMaxLength);
}
