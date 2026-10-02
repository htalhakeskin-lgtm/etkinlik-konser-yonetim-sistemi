using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

// The kind is not sent on an edit; whether a first and last name are needed depends on the stored party, so
// the handler checks them and this only bounds what is sent.
internal sealed class EditPartyValidator : AbstractValidator<EditPartyCommand>
{
    public EditPartyValidator() => this.ValidateParty(_ => false);
}
