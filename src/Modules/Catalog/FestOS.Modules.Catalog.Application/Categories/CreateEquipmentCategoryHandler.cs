using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

internal sealed class CreateEquipmentCategoryHandler(IEquipmentCategoryRepository categories)
    : ICommandHandler<CreateEquipmentCategoryCommand, EquipmentCategoryId>
{
    public async Task<EquipmentCategoryId> HandleAsync(
        CreateEquipmentCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        var tree = new CategoryTree(await categories.LockTreeAsync(cancellationToken));
        CategoryRules.EnsureParentFits(tree, command.ParentId, movedCategory: null);
        var category = EquipmentCategory.Create(command.Name, command.ParentId);
        categories.Add(category);
        return category.Id;
    }
}
