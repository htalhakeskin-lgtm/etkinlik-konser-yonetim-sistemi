using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class CreateKitHandler(IKitRepository kits) : ICommandHandler<CreateKitCommand, KitId>
{
    public async Task<KitId> HandleAsync(CreateKitCommand command, CancellationToken cancellationToken)
    {
        var kit = Kit.Create(command.Name, []);
        await kits.EnsureLinesFitAsync(kit.Id, command.Lines, keptLines: [], cancellationToken);
        kit.Edit(command.Name, command.Lines);
        kits.Add(kit);
        return kit.Id;
    }
}
