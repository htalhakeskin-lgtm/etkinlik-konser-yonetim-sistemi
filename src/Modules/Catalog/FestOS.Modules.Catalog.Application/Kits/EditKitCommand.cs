using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Renames a kit and sets its lines (US-EQP-005, BR-EQP-003).</summary>
public sealed record EditKitCommand(KitId Id, string Name, IReadOnlyList<KitLineDetails> Lines)
    : ICommand<bool>,
        IKitDescription;
