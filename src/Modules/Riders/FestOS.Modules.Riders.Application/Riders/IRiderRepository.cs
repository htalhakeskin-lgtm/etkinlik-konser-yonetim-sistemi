using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>Loads riders and adds their versions for commands (identity ID-01); the unit of work saves them.</summary>
public interface IRiderRepository
{
    /// <summary>The rider, or <see langword="null"/>.</summary>
    Task<Rider?> FindAsync(RiderId id, CancellationToken cancellationToken);

    /// <summary>Adds a new version.</summary>
    void AddVersion(RiderVersion version);
}
