using FestOS.Modules.Inventory.Domain.Warehouses;
using FluentValidation;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>The checks a warehouse's name, city and address get when created or edited (inventory §5).</summary>
internal static class WarehouseCommandValidation
{
    public static void ValidateWarehouse<T>(
        this AbstractValidator<T> validator,
        Func<T, string> name,
        Func<T, string> city,
        Func<T, string> address
    )
    {
        validator
            .RuleFor(command => name(command))
            .NotEmpty()
            .MaximumLength(Warehouse.NameMaxLength)
            .OverridePropertyName("Name");
        validator
            .RuleFor(command => city(command))
            .NotEmpty()
            .MaximumLength(Warehouse.CityMaxLength)
            .OverridePropertyName("City");
        validator
            .RuleFor(command => address(command))
            .NotEmpty()
            .MaximumLength(Warehouse.AddressMaxLength)
            .OverridePropertyName("Address");
    }
}
