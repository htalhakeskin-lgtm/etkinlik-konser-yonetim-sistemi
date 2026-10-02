using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

internal sealed class DeactivateEquipmentCategoryHandler(
    IEquipmentCategoryRepository categories,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateEquipmentCategoryCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateEquipmentCategoryCommand command, CancellationToken cancellationToken)
    {
        EquipmentCategory category = await categories.LoadForChangeAsync(
            command.Id,
            expectedVersion,
            cancellationToken
        );
        if (category.DeactivatedAt is not null)
        {
            return true;
        }

        // Whatever is active under it is moved or deactivated first (catalog CT-06); the models join in 2a.
        var tree = new CategoryTree(await categories.LockTreeAsync(cancellationToken));
        int activeCategories = tree.ActiveChildCount(category.Id);
        if (activeCategories > 0)
        {
            throw CategoryRules.Violation(
                "A category with active categories under it stays active.",
                "activeChildren",
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["activeCategories"] = activeCategories,
                    ["activeModels"] = 0,
                }
            );
        }

        category.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
