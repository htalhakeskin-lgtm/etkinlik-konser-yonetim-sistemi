using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>One kit with its lines, its contents opened down to models and its totals.</summary>
public sealed record GetKitQuery(KitId Id) : IQuery<KitDetails>;
