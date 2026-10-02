using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class ActivateEquipmentModelHandler(
    IEquipmentModelRepository models,
    IEquipmentCategoryRepository categories,
    ExpectedVersion expectedVersion
) : ICommandHandler<ActivateEquipmentModelCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateEquipmentModelCommand command, CancellationToken cancellationToken)
    {
        EquipmentModel model = await models.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        await categories.EnsureActiveCategoryAsync(model.CategoryId, cancellationToken);
        model.Activate();
        return true;
    }
}
