using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Loads and adds models for commands (identity ID-01); the unit of work saves them.</summary>
public interface IEquipmentModelRepository
{
    /// <summary>The model, or <see langword="null"/>.</summary>
    Task<EquipmentModel?> FindAsync(EquipmentModelId id, CancellationToken cancellationToken);

    /// <summary>Adds a new model.</summary>
    void Add(EquipmentModel model);
}
