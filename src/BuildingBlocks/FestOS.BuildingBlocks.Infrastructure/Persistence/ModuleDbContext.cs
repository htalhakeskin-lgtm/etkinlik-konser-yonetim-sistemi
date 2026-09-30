using System.Collections.ObjectModel;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Events;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The base of every module's database context: the module's schema and the shared model rules
/// (building-blocks §5.3). Table mappings are the <c>…Configuration</c> classes of the module's
/// Infrastructure assembly.
/// </summary>
public abstract class ModuleDbContext : DbContext
{
    private bool _saving;

    /// <summary>Creates the context for the module's schema.</summary>
    protected ModuleDbContext(DbContextOptions options, string schema)
        : base(options)
    {
        Schema = schema;
    }

    /// <summary>The module's schema; also the default schema of its tables.</summary>
    public string Schema { get; }

    /// <summary>
    /// The module's check, unique and exclusion constraints by name, mapped to the rule each one enforces,
    /// so a violation reaches the user as that rule (database §12.1).
    /// </summary>
    protected virtual IReadOnlyDictionary<string, string> ConstraintRules => ReadOnlyDictionary<string, string>.Empty;

    /// <summary>Not supported: the save steps are asynchronous, so saving always goes through <see cref="SaveChangesAsync(bool, CancellationToken)"/>.</summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new InvalidOperationException("Module contexts save with SaveChangesAsync, which runs the save steps.");

    /// <summary>
    /// Runs the save steps (building-blocks §4): domain event handlers, aggregate versions and audit
    /// fields, then writes. A stale version becomes <see cref="ConcurrencyConflictException"/>, a mapped
    /// constraint violation a <see cref="BusinessRuleViolationException"/>.
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        if (_saving)
        {
            throw new InvalidOperationException(
                "Domain event handlers must not save; the save that raised the event writes their changes."
            );
        }

        _saving = true;
        try
        {
            await this.GetService<SaveChangesPipeline>().BeforeSaveAsync(this, cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            string changed = exception.Entries.Count > 0 ? exception.Entries[0].Metadata.DisplayName() : "A record";
            throw new ConcurrencyConflictException(
                $"{changed} was changed by someone else after it was loaded.",
                exception
            );
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { ConstraintName: { } constraint }
                && ConstraintRules.TryGetValue(constraint, out string? ruleCode)
            )
        {
            throw new BusinessRuleViolationException(
                ruleCode,
                $"The database constraint {constraint} was violated.",
                innerException: exception
            );
        }
        finally
        {
            _saving = false;
        }
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.IgnoreAny<IDomainEvent>();
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);

        foreach (Type idType in StronglyTypedIdConverter.FindIdTypes(GetType().Assembly))
        {
            configurationBuilder.Properties(idType).HaveConversion(StronglyTypedIdConverter.ConverterTypeFor(idType));
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        MessagingModel.Configure(modelBuilder);
        IdempotencyModel.Configure(modelBuilder);
        AuditEntryModel.Configure(
            modelBuilder,
            ownsTable: string.Equals(Schema, AuditEntry.SchemaName, StringComparison.Ordinal)
        );
        ModelConventions.Apply(modelBuilder);
    }
}
