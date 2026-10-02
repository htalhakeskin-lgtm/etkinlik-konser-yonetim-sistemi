using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

internal sealed class DeactivateEquipmentModelHandler(
    IEquipmentModelRepository models,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateEquipmentModelCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateEquipmentModelCommand command, CancellationToken cancellationToken)
    {
        EquipmentModel model = await models.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        model.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
