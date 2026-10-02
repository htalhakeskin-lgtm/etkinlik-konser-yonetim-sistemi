using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class ActivateKitHandler(IKitRepository kits, ExpectedVersion expectedVersion)
    : ICommandHandler<ActivateKitCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateKitCommand command, CancellationToken cancellationToken)
    {
        Kit kit = await kits.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        kit.Activate();
        return true;
    }
}
