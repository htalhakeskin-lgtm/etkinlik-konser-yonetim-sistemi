using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

internal sealed class EditEquipmentCategoryHandler(
    IEquipmentCategoryRepository categories,
    ExpectedVersion expectedVersion
) : ICommandHandler<EditEquipmentCategoryCommand, bool>
{
    public async Task<bool> HandleAsync(EditEquipmentCategoryCommand command, CancellationToken cancellationToken)
    {
        EquipmentCategory category = await categories.LoadForChangeAsync(
            command.Id,
            expectedVersion,
            cancellationToken
        );
        if (category.ParentId != command.ParentId)
        {
            var tree = new CategoryTree(await categories.LockTreeAsync(cancellationToken));
            CategoryRules.EnsureParentFits(tree, command.ParentId, category.Id);
        }

        category.Edit(command.Name, command.ParentId);
        return true;
    }
}
