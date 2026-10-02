using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class ActivateProductionHandler(IProductionRepository productions, ExpectedVersion expectedVersion)
    : ICommandHandler<ActivateProductionCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateProductionCommand command, CancellationToken cancellationToken)
    {
        Production production = await productions.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        production.Activate();
        return true;
    }
}
