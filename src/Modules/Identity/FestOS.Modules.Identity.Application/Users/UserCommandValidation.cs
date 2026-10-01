using FestOS.Modules.Identity.Domain.Users;
using FluentValidation;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>The checks a user's name, email, roles and warehouses get when created or edited (identity §7).</summary>
internal static class UserCommandValidation
{
    public static void ValidateUser<T>(
        this AbstractValidator<T> validator,
        Func<T, string> fullName,
        Func<T, string> email,
        Func<T, IReadOnlyList<Role>> roles,
        Func<T, IReadOnlyList<Guid>> warehouseIds
    )
    {
        validator
            .RuleFor(command => fullName(command))
            .NotEmpty()
            .MaximumLength(User.FullNameMaxLength)
            .OverridePropertyName("FullName");
        validator
            .RuleFor(command => email(command))
            .NotEmpty()
            .MaximumLength(EmailAddress.MaxLength)
            .EmailAddress()
            .OverridePropertyName("Email");
        validator.RuleFor(command => roles(command)).NotEmpty().OverridePropertyName("Roles");
        validator.RuleForEach(command => roles(command)).IsInEnum().OverridePropertyName("Roles");
        validator.RuleForEach(command => warehouseIds(command)).NotEmpty().OverridePropertyName("WarehouseIds");
    }
}
