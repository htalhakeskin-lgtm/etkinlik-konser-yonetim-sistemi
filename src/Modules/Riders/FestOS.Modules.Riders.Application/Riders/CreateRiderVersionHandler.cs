using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

internal sealed class CreateRiderVersionHandler(
    IRiderRepository riders,
    ICatalogDirectory catalog,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion
) : ICommandHandler<CreateRiderVersionCommand, RiderVersionId>
{
    public async Task<RiderVersionId> HandleAsync(
        CreateRiderVersionCommand command,
        CancellationToken cancellationToken
    )
    {
        Rider rider =
            await riders.FindAsync(command.RiderId, cancellationToken)
            ?? throw new NotFoundException("Rider", command.RiderId.Value);
        expectedVersion.EnsureMatches(rider);

        // The rules come first, so a malformed line is not looked up in Catalog.
        RiderLineRules.EnsureValid(command.Lines);
        await catalog.EnsureTargetsActiveAsync(command.Lines, cancellationToken);

        RiderVersion version = rider.AddVersion(command.Lines, command.Note, currentUser.DisplayName);
        riders.AddVersion(version);
        return version.Id;
    }
}
