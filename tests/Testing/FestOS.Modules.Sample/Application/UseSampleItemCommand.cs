using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;

namespace FestOS.Modules.Sample.Application;

/// <summary>Puts an item in use, which publishes an integration event.</summary>
internal sealed record UseSampleItemCommand(SampleItemId SampleItemId) : ICommand<bool>;
