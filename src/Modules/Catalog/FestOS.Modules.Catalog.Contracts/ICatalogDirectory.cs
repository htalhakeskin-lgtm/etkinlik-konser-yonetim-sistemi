namespace FestOS.Modules.Catalog.Contracts;

/// <summary>
/// What other modules may ask Catalog synchronously (05 §5.4): the models and categories with these
/// identifiers, to check a choice or to show names (parties MD-02). Kits join with the kits.
/// </summary>
public interface ICatalogDirectory
{
    /// <summary>The models among <paramref name="ids"/> that exist, by identifier.</summary>
    Task<IReadOnlyDictionary<Guid, ModelSummary>> FindModelsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    );

    /// <summary>The categories among <paramref name="ids"/> that exist, by identifier.</summary>
    Task<IReadOnlyDictionary<Guid, CategorySummary>> FindCategoriesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    );
}
