using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Defines a kit with its lines (US-EQP-005).</summary>
public sealed record CreateKitCommand(string Name, IReadOnlyList<KitLineDetails> Lines)
    : ICommand<KitId>,
        IKitDescription;
