using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Loads the model a command changes, at the version the request saw (api §9).</summary>
internal static class EquipmentModelLoader
{
    public static async Task<EquipmentModel> LoadForChangeAsync(
        this IEquipmentModelRepository models,
        EquipmentModelId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        EquipmentModel model =
            await models.FindAsync(id, cancellationToken) ?? throw new NotFoundException("EquipmentModel", id.Value);
        expectedVersion.EnsureMatches(model);
        return model;
    }
}
