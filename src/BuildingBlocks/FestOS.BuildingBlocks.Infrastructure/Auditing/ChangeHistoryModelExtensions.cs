using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// Keeps an entity type the module does not own out of the change history, such as ASP.NET's data
/// protection keys, which cannot carry <c>[NotAudited]</c>.
/// </summary>
public static class ChangeHistoryModelExtensions
{
    /// <summary>The model annotation the change history writer looks for.</summary>
    public const string NotAuditedAnnotation = "FestOS:NotAudited";

    /// <summary>Keeps the entity type out of the change history.</summary>
    public static EntityTypeBuilder<TEntity> ExcludeFromChangeHistory<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(NotAuditedAnnotation, true);
    }
}
