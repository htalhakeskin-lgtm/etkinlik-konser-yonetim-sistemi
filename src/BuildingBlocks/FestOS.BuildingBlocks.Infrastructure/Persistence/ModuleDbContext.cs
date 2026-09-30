using FestOS.BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The base of every module's database context: the module's schema and the shared model rules
/// (building-blocks §5.3). Table mappings are the <c>…Configuration</c> classes of the module's
/// Infrastructure assembly.
/// </summary>
public abstract class ModuleDbContext : DbContext
{
    /// <summary>Creates the context for the module's schema.</summary>
    protected ModuleDbContext(DbContextOptions options, string schema)
        : base(options)
    {
        Schema = schema;
    }

    /// <summary>The module's schema; also the default schema of its tables.</summary>
    public string Schema { get; }

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
        ModelConventions.Apply(modelBuilder);
    }
}
