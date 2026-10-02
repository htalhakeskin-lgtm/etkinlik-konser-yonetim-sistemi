using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class EditKitHandler(IKitRepository kits, ExpectedVersion expectedVersion)
    : ICommandHandler<EditKitCommand, bool>
{
    public async Task<bool> HandleAsync(EditKitCommand command, CancellationToken cancellationToken)
    {
        Kit kit = await kits.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        await kits.EnsureLinesFitAsync(kit.Id, command.Lines, kit.Lines, cancellationToken);
        kit.Edit(command.Name, command.Lines);
        return true;
    }
}
