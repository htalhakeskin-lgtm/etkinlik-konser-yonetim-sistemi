namespace FestOS.Modules.Catalog.Infrastructure.Kits;

/// <summary>A kit line in a kit's body: a model or another kit, and how many.</summary>
public sealed record KitLineRequest(int Quantity, Guid? ModelId = null, Guid? SubKitId = null);
