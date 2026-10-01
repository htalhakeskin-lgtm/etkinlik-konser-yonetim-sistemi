using FestOS.Modules.Identity.Domain.Users;
using FluentValidation;

namespace FestOS.Modules.Identity.Application.Passwords;

internal sealed class ChangeMyPasswordValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordValidator()
    {
        RuleFor(command => command.CurrentPassword).MaximumLength(PasswordPolicy.MaxLength * 2);
        RuleFor(command => command.NewPassword).NotEmpty().MaximumLength(PasswordPolicy.MaxLength * 2);
    }
}
