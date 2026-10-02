using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class EditProductionHandler(IProductionRepository productions, ExpectedVersion expectedVersion)
    : ICommandHandler<EditProductionCommand, bool>
{
    public async Task<bool> HandleAsync(EditProductionCommand command, CancellationToken cancellationToken)
    {
        Production production = await productions.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        production.Edit(command.Name, command.Description);
        return true;
    }
}
