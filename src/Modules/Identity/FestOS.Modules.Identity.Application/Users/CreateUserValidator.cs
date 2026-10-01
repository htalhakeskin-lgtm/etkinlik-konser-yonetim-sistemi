using FluentValidation;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator() =>
        this.ValidateUser(
            command => command.FullName,
            command => command.Email,
            command => command.Roles,
            command => command.WarehouseIds
        );
}
