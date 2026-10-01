using FluentValidation;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class EditWarehouseValidator : AbstractValidator<EditWarehouseCommand>
{
    public EditWarehouseValidator() =>
        this.ValidateWarehouse(command => command.Name, command => command.City, command => command.Address);
}
