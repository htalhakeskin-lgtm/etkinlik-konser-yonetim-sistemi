using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Opens a deactivated kit again.</summary>
public sealed record ActivateKitCommand(KitId Id) : ICommand<bool>;
