using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;

namespace FestOS.Modules.Sample.Application;

/// <summary>Creates an item.</summary>
internal sealed record CreateSampleItemCommand(string Name, decimal UnitPrice) : ICommand<SampleItemId>;
