using FestOS.Modules.Parties.Domain.Parties;
using FluentValidation;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>
/// The field checks a party gets when created or edited (parties §8). Rules stay with the party: a missing
/// role or a second primary answers with its rule number (BR-PTY-001, BR-PTY-002).
/// </summary>
internal static class PartyValidation
{
    /// <summary>The longest phone number.</summary>
    public const int PhoneMaxLength = 32;

    /// <summary>The longest e-mail address.</summary>
    public const int EmailMaxLength = 254;

    /// <summary>The longest postal address.</summary>
    public const int AddressMaxLength = 500;

    public static void ValidateParty<T>(this AbstractValidator<T> validator, Func<T, bool> isPerson)
        where T : IPartyDescription
    {
        validator.RuleFor(party => party.Name).NotEmpty().MaximumLength(Party.NameMaxLength);
        validator
            .RuleFor(party => party.FirstName)
            .NotEmpty()
            .MaximumLength(Party.PersonNameMaxLength)
            .When(party => isPerson(party));
        validator
            .RuleFor(party => party.LastName)
            .NotEmpty()
            .MaximumLength(Party.PersonNameMaxLength)
            .When(party => isPerson(party));
        validator.RuleFor(party => party.FirstName).MaximumLength(Party.PersonNameMaxLength);
        validator.RuleFor(party => party.LastName).MaximumLength(Party.PersonNameMaxLength);
        validator.RuleFor(party => party.LegalName).MaximumLength(Party.NameMaxLength);
        validator.RuleFor(party => party.Roles).NotNull();
        validator.RuleForEach(party => party.Roles).IsInEnum();
        validator
            .RuleFor(party => party.ContactPoints)
            .NotNull()
            .Must(points => points.Count <= Party.MaxContactPoints);
        validator
            .RuleForEach(party => party.ContactPoints)
            .ChildRules(point =>
            {
                point.RuleFor(details => details.Kind).IsInEnum();
                point.RuleFor(details => details.Label).MaximumLength(ContactPoint.LabelMaxLength);
                point.RuleFor(details => details.Value).NotEmpty();
                point
                    .RuleFor(details => details.Value)
                    .MaximumLength(PhoneMaxLength)
                    .Matches(@"^\+?[0-9 ()\-]+$")
                    .Must(value => value.Count(char.IsAsciiDigit) is >= 7 and <= 15)
                    .When(details => details.Kind == ContactPointKind.Phone);
                point
                    .RuleFor(details => details.Value)
                    .MaximumLength(EmailMaxLength)
                    .EmailAddress()
                    .When(details => details.Kind == ContactPointKind.Email);
                point
                    .RuleFor(details => details.Value)
                    .MaximumLength(AddressMaxLength)
                    .When(details => details.Kind == ContactPointKind.Address);
            });
    }
}
