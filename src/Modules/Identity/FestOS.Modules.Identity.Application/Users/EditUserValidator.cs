using FluentValidation;

namespace FestOS.Modules.Identity.Application.Users;

internal sealed class EditUserValidator : AbstractValidator<EditUserCommand>
{
    public EditUserValidator() =>
        this.ValidateUser(
            command => command.FullName,
            command => command.Email,
            command => command.Roles,
            command => command.WarehouseIds
        );
}
