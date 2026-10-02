using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Takes a kit out of new selections (BR-SYS-001).</summary>
public sealed record DeactivateKitCommand(KitId Id) : ICommand<bool>;
