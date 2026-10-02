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

        // Whatever is active under it is moved or deactivated first (catalog CT-06). Model commands take the same
        // lock, so a model cannot join the category meanwhile.
        var tree = new CategoryTree(await categories.LockTreeAsync(cancellationToken));
        int activeCategories = tree.ActiveChildCount(category.Id);
        int activeModels = await categories.CountActiveModelsAsync(category.Id, cancellationToken);
        if (activeCategories > 0 || activeModels > 0)
        {
            throw CategoryRules.Violation(
                "A category with active categories or models under it stays active.",
                "activeChildren",
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["activeCategories"] = activeCategories,
                    ["activeModels"] = activeModels,
                }
            );
        }

        category.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
