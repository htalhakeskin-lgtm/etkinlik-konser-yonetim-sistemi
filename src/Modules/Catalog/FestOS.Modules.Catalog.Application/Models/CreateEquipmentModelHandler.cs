using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class CreateEquipmentModelHandler(
    IEquipmentModelRepository models,
    IEquipmentCategoryRepository categories
) : ICommandHandler<CreateEquipmentModelCommand, EquipmentModelId>
{
    public async Task<EquipmentModelId> HandleAsync(
        CreateEquipmentModelCommand command,
        CancellationToken cancellationToken
    )
    {
        await categories.EnsureActiveCategoryAsync(command.CategoryId, cancellationToken);
        var model = EquipmentModel.Create(
            command.Brand,
            command.Name,
            command.CategoryId,
            command.TrackingType,
            command.Measures
        );
        models.Add(model);
        return model.Id;
    }
}
