using FestOS.Modules.Identity.Domain.Users;
using FluentValidation;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class CreateFirstAdministratorValidator : AbstractValidator<CreateFirstAdministratorCommand>
{
    public CreateFirstAdministratorValidator()
    {
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(User.FullNameMaxLength);
        RuleFor(command => command.Email).NotEmpty().MaximumLength(EmailAddress.MaxLength).EmailAddress();
    }
}
