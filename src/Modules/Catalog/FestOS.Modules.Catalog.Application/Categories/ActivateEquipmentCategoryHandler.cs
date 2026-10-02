using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

internal sealed class ActivateEquipmentCategoryHandler(
    IEquipmentCategoryRepository categories,
    ExpectedVersion expectedVersion
) : ICommandHandler<ActivateEquipmentCategoryCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateEquipmentCategoryCommand command, CancellationToken cancellationToken)
    {
        EquipmentCategory category = await categories.LoadForChangeAsync(
            command.Id,
            expectedVersion,
            cancellationToken
        );
        var tree = new CategoryTree(await categories.LockTreeAsync(cancellationToken));
        CategoryRules.EnsureParentFits(tree, category.ParentId, movedCategory: null);
        category.Activate();
        return true;
    }
}
