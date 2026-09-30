using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Http.Json;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using FestOS.BuildingBlocks.Infrastructure.Messaging;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The innermost command decorator (building-blocks §4, step 7): runs the handler and the save in one
/// transaction of the command's module, inside the retry strategy (database §11.4). A request with an
/// idempotency key stores the key first and the result last, in the same transaction; a retry of a
/// finished request gets the stored result without running the handler again (api §10).
/// </summary>
internal sealed class UnitOfWorkCommandDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IServiceProvider services,
    Outbox outbox,
    IdempotencyRequest idempotency,
    ActingUser actingUser,
    TimeProvider timeProvider
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        ModuleDbContext context = ModuleUnitOfWork.ContextFor(services, typeof(TCommand));
        return idempotency.TryClaim(out Guid key, out string fingerprint)
            ? ModuleUnitOfWork.RunAsync(
                context,
                outbox,
                retryCancellationToken =>
                    HandleOnceAsync(context, command, key, fingerprint, actingUser.UserId, retryCancellationToken),
                cancellationToken
            )
            : ModuleUnitOfWork.RunAsync(
                context,
                outbox,
                retryCancellationToken => inner.HandleAsync(command, retryCancellationToken),
                cancellationToken
            );
    }

    private async Task<TResult> HandleOnceAsync(
        ModuleDbContext context,
        TCommand command,
        Guid key,
        string fingerprint,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        string? stored = await IdempotencyKeys.StoreAsync(
            context,
            userId,
            key,
            fingerprint,
            timeProvider.GetUtcNow(),
            cancellationToken
        );
        if (stored is not null)
        {
            idempotency.MarkReplayed();
            return JsonSerializer.Deserialize<TResult>(stored, ApiJson.Options)!;
        }

        TResult result = await inner.HandleAsync(command, cancellationToken);
        await IdempotencyKeys.SaveResultAsync(
            context,
            userId,
            key,
            JsonSerializer.Serialize(result, ApiJson.Options),
            cancellationToken
        );
        return result;
    }
}
