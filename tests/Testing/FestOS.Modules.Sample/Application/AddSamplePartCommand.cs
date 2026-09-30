using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;

namespace FestOS.Modules.Sample.Application;

/// <summary>Adds a part; with <paramref name="FailAfterSaving"/> the handler fails after writing, to test the rollback.</summary>
internal sealed record AddSamplePartCommand(SampleItemId SampleItemId, string Label, bool FailAfterSaving = false)
    : ICommand<SampleItemPartId>;
