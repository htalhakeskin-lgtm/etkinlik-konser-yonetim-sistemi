using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Loads the production a command changes, at the version the request saw (api §9).</summary>
internal static class ProductionLoader
{
    public static async Task<Production> LoadForChangeAsync(
        this IProductionRepository productions,
        ProductionId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        Production production =
            await productions.FindAsync(id, cancellationToken) ?? throw new NotFoundException("Production", id.Value);
        expectedVersion.EnsureMatches(production);
        return production;
    }
}
