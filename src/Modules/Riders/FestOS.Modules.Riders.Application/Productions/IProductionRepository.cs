using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Loads and adds productions for commands (identity ID-01); the unit of work saves them.</summary>
public interface IProductionRepository
{
    /// <summary>The production, or <see langword="null"/>.</summary>
    Task<Production?> FindAsync(ProductionId id, CancellationToken cancellationToken);

    /// <summary>Adds a new production with its empty rider (riders RD-01).</summary>
    void Add(Production production, Rider rider);
}
