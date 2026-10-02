using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>A kit that holds a model directly, for the model's page.</summary>
public sealed record KitReference(KitId Id, string Name, bool IsActive);
