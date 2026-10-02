using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>What creating and editing a kit both send (catalog §6).</summary>
public interface IKitDescription
{
    /// <summary>The name.</summary>
    string Name { get; }

    /// <summary>The lines in their order.</summary>
    IReadOnlyList<KitLineDetails> Lines { get; }
}
