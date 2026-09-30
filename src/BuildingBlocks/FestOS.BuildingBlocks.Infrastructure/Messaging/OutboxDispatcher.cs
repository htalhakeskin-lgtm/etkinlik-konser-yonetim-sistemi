using System.Threading.Channels;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// One delivery loop per module (building-blocks §7): delivers what is waiting, then sleeps until a
/// commit signals new events or the poll interval passes.
/// </summary>
internal sealed partial class OutboxDispatcher(
    ModuleCatalog modules,
    OutboxProcessor processor,
    OutboxSignals signals,
    IOptions<MessagingOptions> options,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<OutboxDispatcher> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The Host can run without a database in development, e.g. to work on the front end.
        if (configuration.GetConnectionString(DatabaseConnections.ConnectionStringName) is null)
        {
            LogDisabled(logger);
            return Task.CompletedTask;
        }

        return Task.WhenAll(modules.Modules.Select(module => RunAsync(module, stoppingToken)));
    }

    private async Task RunAsync(IModuleDefinition module, CancellationToken stoppingToken)
    {
        ChannelReader<bool> signal = signals.ReaderFor(module.Schema);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // A full batch means more may be waiting.
                while (await processor.ProcessBatchAsync(module.Name, stoppingToken) == OutboxProcessor.BatchSize) { }

                await processor.CountPendingAsync(module.Name, stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogPassFailed(logger, exception, module.Name);
            }

            await WaitAsync(signal, stoppingToken);
        }
    }

    private async Task WaitAsync(ChannelReader<bool> signal, CancellationToken stoppingToken)
    {
        using var wait = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        await Task.WhenAny(
            Task.Delay(options.Value.PollInterval, timeProvider, wait.Token),
            signal.WaitToReadAsync(wait.Token).AsTask()
        );
        await wait.CancelAsync();
        signal.TryRead(out _);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event delivery is off: the database connection string is not configured"
    )]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Delivering the events of module {ModuleName} failed")]
    private static partial void LogPassFailed(ILogger logger, Exception exception, string moduleName);
}
