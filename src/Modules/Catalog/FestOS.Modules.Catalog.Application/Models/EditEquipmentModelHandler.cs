using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class EditEquipmentModelHandler(
    IEquipmentModelRepository models,
    IEquipmentCategoryRepository categories,
    ExpectedVersion expectedVersion
) : ICommandHandler<EditEquipmentModelCommand, bool>
{
    public async Task<bool> HandleAsync(EditEquipmentModelCommand command, CancellationToken cancellationToken)
    {
        EquipmentModel model = await models.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        if (model.CategoryId != command.CategoryId && model.DeactivatedAt is null)
        {
            await categories.EnsureActiveCategoryAsync(command.CategoryId, cancellationToken);
        }

        model.Edit(command.Brand, command.Name, command.CategoryId, command.TrackingType, command.Measures);
        return true;
    }
}
