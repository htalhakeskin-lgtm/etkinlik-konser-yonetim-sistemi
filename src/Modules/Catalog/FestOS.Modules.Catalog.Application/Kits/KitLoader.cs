using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Loads the kit a command changes, at the version the request saw (api §9).</summary>
internal static class KitLoader
{
    public static async Task<Kit> LoadForChangeAsync(
        this IKitRepository kits,
        KitId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        Kit kit = await kits.FindAsync(id, cancellationToken) ?? throw new NotFoundException("Kit", id.Value);
        expectedVersion.EnsureMatches(kit);
        return kit;
    }
}
