using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Contracts;
using FluentValidation;

// A stand-in for a module's Application layer; the namespace follows FestOS.Modules.{Module}.….
#pragma warning disable IDE0130 // The namespace mimics a module on purpose; the decorators read the module name from it.
#pragma warning disable MA0048 // The sample module's small types stay together in one fixture file.
namespace FestOS.Modules.Sample.Application;

internal sealed record PlaceSampleCommand(string Name) : ICommand<Guid>;

internal sealed class PlaceSampleHandler(SampleHandlerProbe probe) : ICommandHandler<PlaceSampleCommand, Guid>
{
    public Task<Guid> HandleAsync(PlaceSampleCommand command, CancellationToken cancellationToken) =>
        probe.RunAsync(Guid.CreateVersion7());
}

internal sealed class PlaceSampleValidator : AbstractValidator<PlaceSampleCommand>
{
    public PlaceSampleValidator() => RuleFor(command => command.Name).NotEmpty().MaximumLength(10);
}

internal sealed record GetSampleQuery(Guid SampleId) : IQuery<string>;

internal sealed class GetSampleHandler(SampleHandlerProbe probe) : IQueryHandler<GetSampleQuery, string>
{
    public Task<string> HandleAsync(GetSampleQuery query, CancellationToken cancellationToken) =>
        probe.RunAsync("sample");
}

internal sealed class GetSampleValidator : AbstractValidator<GetSampleQuery>
{
    public GetSampleValidator() => RuleFor(query => query.SampleId).NotEmpty();
}

internal sealed record SamplePlacedIntegrationEvent : IntegrationEvent;

internal sealed class NoteOnSamplePlacedHandler : IIntegrationEventHandler<SamplePlacedIntegrationEvent>
{
    public Task HandleAsync(SamplePlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

/// <summary>Counts handler calls and lets a test make the handlers fail.</summary>
internal sealed class SampleHandlerProbe
{
    public int Calls { get; private set; }

    public Exception? Failure { get; set; }

    public Task<T> RunAsync<T>(T result)
    {
        Calls++;
        return Failure is null ? Task.FromResult(result) : Task.FromException<T>(Failure);
    }
}
