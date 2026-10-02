using FestOS.Modules.Parties.Domain.Parties;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class CreatePartyValidator : AbstractValidator<CreatePartyCommand>
{
    public CreatePartyValidator()
    {
        RuleFor(command => command.Kind).IsInEnum();
        this.ValidateParty(command => command.Kind == PartyKind.Person);
    }
}
