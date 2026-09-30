using System.Reflection;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The steps <see cref="ModuleDbContext"/> runs before writing (building-blocks §4): domain events,
/// aggregate versions and audit fields. Scoped, so handlers share the request's context.
/// </summary>
internal sealed class SaveChangesPipeline(
    IServiceProvider services,
    ICurrentUser currentUser,
    TimeProvider timeProvider
)
{
    /// <summary>Handlers may raise new events; more rounds than this means a loop (building-blocks BB-06).</summary>
    public const int MaxDomainEventRounds = 5;

    private static readonly MethodInfo PublishMethod = typeof(SaveChangesPipeline).GetMethod(
        nameof(PublishAsync),
        BindingFlags.NonPublic | BindingFlags.Static
    )!;

    public async Task BeforeSaveAsync(ModuleDbContext context, CancellationToken cancellationToken)
    {
        await DispatchDomainEventsAsync(context, cancellationToken);
        StampChangedAggregates(context);
    }

    // Step 1: handlers run in the same transaction and may change more of the module's data.
    private async Task DispatchDomainEventsAsync(ModuleDbContext context, CancellationToken cancellationToken)
    {
        for (int round = 1; round <= MaxDomainEventRounds; round++)
        {
            List<IDomainEvent> domainEvents = DequeueDomainEvents(context);
            if (domainEvents.Count == 0)
            {
                return;
            }

            foreach (IDomainEvent domainEvent in domainEvents)
            {
                await (Task)
                    PublishMethod
                        .MakeGenericMethod(domainEvent.GetType())
                        .Invoke(null, [services, domainEvent, cancellationToken])!;
            }
        }

        if (DequeueDomainEvents(context).Count > 0)
        {
            throw new InvalidOperationException(
                $"Domain event handlers were still raising events after {MaxDomainEventRounds} rounds."
            );
        }
    }

    private static List<IDomainEvent> DequeueDomainEvents(ModuleDbContext context) =>
        [.. context.ChangeTracker.Entries<IAggregateRoot>().SelectMany(entry => entry.Entity.DequeueDomainEvents())];

    private static async Task PublishAsync<TDomainEvent>(
        IServiceProvider services,
        TDomainEvent domainEvent,
        CancellationToken cancellationToken
    )
        where TDomainEvent : IDomainEvent
    {
        foreach (IDomainEventHandler<TDomainEvent> handler in services.GetServices<IDomainEventHandler<TDomainEvent>>())
        {
            await handler.HandleAsync(domainEvent, cancellationToken);
        }
    }

    // Steps 2 and 3: every aggregate that changed, itself or through a child entity, gets a new
    // version and its audit fields (database §9, §11.1).
    private void StampChangedAggregates(ModuleDbContext context)
    {
        List<EntityEntry> entries = [.. context.ChangeTracker.Entries()];
        HashSet<object> changedRoots = new(ReferenceEqualityComparer.Instance);

        foreach (
            EntityEntry entry in entries.Where(entry =>
                entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            )
        )
        {
            if (FindAggregateRoot(entry, entries) is { State: not EntityState.Deleted } root)
            {
                changedRoots.Add(root.Entity);
            }
        }

        // PostgreSQL keeps microseconds; the stored value then equals the one in memory (database §7.1).
        DateTimeOffset now = timeProvider.GetUtcNow();
        now = now.AddTicks(-(now.Ticks % 10));

        foreach (EntityEntry root in changedRoots.Select(context.Entry))
        {
            PropertyEntry version = root.Property(nameof(IAggregateRoot.Version));
            if (root.State == EntityState.Added)
            {
                version.CurrentValue = 1;
                root.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                root.Property(nameof(IAuditable.CreatedBy)).CurrentValue = currentUser.UserId;
            }
            else
            {
                version.CurrentValue = (int)version.OriginalValue! + 1;
            }

            root.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
            root.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = currentUser.UserId;
        }
    }

    // A child belongs to the aggregate its cascading foreign key points to: only an aggregate's own
    // children cascade (V-11). Entities outside any aggregate return null.
    private static EntityEntry? FindAggregateRoot(EntityEntry entry, List<EntityEntry> entries)
    {
        EntityEntry current = entry;
        while (current.Entity is not IAggregateRoot)
        {
            IForeignKey? ownership = current
                .Metadata.GetForeignKeys()
                .FirstOrDefault(foreignKey => foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
            if (ownership is null)
            {
                return null;
            }

            object?[] parentKey =
            [
                .. ownership.Properties.Select(property =>
                    current.State == EntityState.Deleted
                        ? current.Property(property.Name).OriginalValue
                        : current.Property(property.Name).CurrentValue
                ),
            ];

            current =
                entries.FirstOrDefault(candidate =>
                    ownership.PrincipalEntityType.IsAssignableFrom(candidate.Metadata)
                    && ownership
                        .PrincipalKey.Properties.Select(property => candidate.Property(property.Name).CurrentValue)
                        .SequenceEqual(parentKey)
                )
                ?? throw new InvalidOperationException(
                    $"{entry.Metadata.DisplayName()} changed without its aggregate being loaded; change it through the aggregate root."
                );
        }

        return current;
    }
}
