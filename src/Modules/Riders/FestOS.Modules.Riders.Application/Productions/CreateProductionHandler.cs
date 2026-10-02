using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class CreateProductionHandler(IProductionRepository productions, IPartyDirectory parties)
    : ICommandHandler<CreateProductionCommand, ProductionId>
{
    public async Task<ProductionId> HandleAsync(CreateProductionCommand command, CancellationToken cancellationToken)
    {
        await parties.EnsureSelectableArtistAsync(command.ArtistPartyId, cancellationToken);
        var production = Production.Create(command.ArtistPartyId, command.Name, command.Description);
        productions.Add(production, Rider.ForProduction(production.Id));
        return production.Id;
    }
}
