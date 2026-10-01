using FestOS.Modules.Identity.Domain.Users;
using FluentValidation;

namespace FestOS.Modules.Identity.Application.Authentication;

internal sealed class SignInValidator : AbstractValidator<SignInCommand>
{
    // No password the policy accepts is longer.
    private const int PasswordMaxLength = 128;

    public SignInValidator()
    {
        RuleFor(command => command.Email).NotEmpty().MaximumLength(EmailAddress.MaxLength);
        RuleFor(command => command.Password).NotEmpty().MaximumLength(PasswordMaxLength);
    }
}
