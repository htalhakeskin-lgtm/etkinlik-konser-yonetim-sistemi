using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Contracts;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Wraps every integration event listener (AT-10): runs it in its own module's unit of work and records
/// the event in that module's inbox in the same transaction, so a redelivered event is skipped
/// (building-blocks BB-03, 05 §9.2).
/// </summary>
internal sealed class InboxIntegrationEventDecorator<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> inner,
    IServiceProvider services,
    Outbox outbox,
    TimeProvider timeProvider
) : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        Type handlerType = inner.GetType();
        string handler = handlerType.FullName ?? handlerType.Name;
        ModuleDbContext context = ModuleUnitOfWork.ContextFor(services, handlerType);

        return ModuleUnitOfWork.RunAsync(
            context,
            outbox,
            async retryCancellationToken =>
            {
                bool handled = await context
                    .Set<InboxMessage>()
                    .AnyAsync(
                        message => message.MessageId == integrationEvent.MessageId && message.Handler == handler,
                        retryCancellationToken
                    );
                if (!handled)
                {
                    await inner.HandleAsync(integrationEvent, retryCancellationToken);
                    context
                        .Set<InboxMessage>()
                        .Add(
                            new InboxMessage
                            {
                                MessageId = integrationEvent.MessageId,
                                Handler = handler,
                                ProcessedAt = timeProvider.GetUtcNow(),
                            }
                        );
                }

                return handled;
            },
            cancellationToken
        );
    }
}
