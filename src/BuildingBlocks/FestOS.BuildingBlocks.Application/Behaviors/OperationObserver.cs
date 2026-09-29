using System.Diagnostics;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Users;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Runs a command or query inside a trace span named after it and logs the outcome
/// (observability §3, §4). Expected outcomes are not errors: they are logged at
/// <see cref="LogLevel.Information"/> and leave the span status unset.
/// </summary>
internal static partial class OperationObserver
{
    public const string ActivitySourceName = "FestOS.BuildingBlocks";

    private static readonly ActivitySource Source = new(ActivitySourceName);

    private static readonly Func<ILogger, string?, Guid, IDisposable?> Scope = LoggerMessage.DefineScope<string?, Guid>(
        "{ModuleName} {UserId}"
    );

    public static async Task<TResult> RunAsync<TResult>(
        OperationName operation,
        Func<Task<TResult>> handle,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        ILogger logger
    )
    {
        using IDisposable? scope = Scope(logger, operation.ModuleName, currentUser.UserId);
        using Activity? activity = Source.StartActivity(operation.Name);
        activity?.SetTag("festos.module", operation.ModuleName);
        long startedAt = timeProvider.GetTimestamp();

        try
        {
            TResult result = await handle().ConfigureAwait(false);
            double elapsedMilliseconds = timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;
            LogCompleted(logger, operation.Name, elapsedMilliseconds);
            return result;
        }
        catch (Exception exception) when (ErrorCodes.Of(exception) is { } errorCode)
        {
            activity?.SetTag("festos.error.code", errorCode);
            LogRejected(logger, operation.Name, errorCode);
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.AddException(exception);
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "{OperationName} completed in {ElapsedMilliseconds} ms")]
    private static partial void LogCompleted(ILogger logger, string operationName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Information, Message = "{OperationName} was rejected with {ErrorCode}")]
    private static partial void LogRejected(ILogger logger, string operationName, string errorCode);
}
