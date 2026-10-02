using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

/// <summary>The body of <c>POST /api/v1/kits</c> and <c>PUT /api/v1/kits/{kitId}</c>: the name and every line.</summary>
public sealed record KitRequest(string Name, IReadOnlyList<KitLineRequest> Lines)
{
    /// <summary>The lines as the kit takes them.</summary>
    public IReadOnlyList<KitLineDetails> LineDetails() =>
        [
            .. Lines.Select(line => new KitLineDetails(
                line.ModelId is { } model ? EquipmentModelId.From(model) : null,
                line.SubKitId is { } kit ? KitId.From(kit) : null,
                line.Quantity
            )),
        ];
}
