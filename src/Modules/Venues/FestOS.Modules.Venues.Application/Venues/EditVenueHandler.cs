using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class EditVenueHandler(
    IVenueRepository venues,
    IPartyDirectory parties,
    ExpectedVersion expectedVersion
) : ICommandHandler<EditVenueCommand, bool>
{
    public async Task<bool> HandleAsync(EditVenueCommand command, CancellationToken cancellationToken)
    {
        Venue venue = await venues.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);

        // A kept operator stays even if it has since lost the role or been deactivated (venues §5).
        if (venue.OperatorPartyId != command.Description.OperatorPartyId)
        {
            await parties.EnsureSelectableOperatorAsync(command.Description.OperatorPartyId, cancellationToken);
        }

        venue.Edit(command.Description);
        return true;
    }
}
