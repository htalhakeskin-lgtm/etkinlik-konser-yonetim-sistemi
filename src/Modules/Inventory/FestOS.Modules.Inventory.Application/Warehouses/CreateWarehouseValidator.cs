using FluentValidation;

namespace FestOS.Modules.Inventory.Application.Warehouses;

internal sealed class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseValidator() =>
        this.ValidateWarehouse(command => command.Name, command => command.City, command => command.Address);
}
