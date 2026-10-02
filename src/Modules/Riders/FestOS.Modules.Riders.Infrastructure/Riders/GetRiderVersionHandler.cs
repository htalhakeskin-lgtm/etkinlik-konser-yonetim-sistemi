using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Riders.Application.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure.Riders;

internal sealed class GetRiderVersionHandler(RidersDbContext context, ICatalogDirectory catalog)
    : IQueryHandler<GetRiderVersionQuery, RiderVersionDetails>
{
    public async Task<RiderVersionDetails> HandleAsync(GetRiderVersionQuery query, CancellationToken cancellationToken)
    {
        RiderVersion version =
            await context
                .RiderVersions.AsNoTracking()
                .Include(found => found.Lines)
                    .ThenInclude(line => line.Equivalents)
                .AsSplitQuery()
                .SingleOrDefaultAsync(found => found.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("RiderVersion", query.Id.Value);

        // The targets' names come from Catalog in two calls (parties MD-02).
        IReadOnlyDictionary<Guid, ModelSummary> models = await catalog.FindModelsAsync(
            [
                .. version
                    .Lines.SelectMany(line =>
                        line.Equivalents.Select(equivalent => equivalent.ModelId).Prepend(line.ModelId ?? Guid.Empty)
                    )
                    .Distinct(),
            ],
            cancellationToken
        );
        IReadOnlyDictionary<Guid, CategorySummary> categories = await catalog.FindCategoriesAsync(
            [.. version.Lines.Select(line => line.CategoryId).OfType<Guid>().Distinct()],
            cancellationToken
        );

        return new RiderVersionDetails(
            version.Id,
            version.RiderId,
            version.Number,
            version.Note,
            version.CreatedAt,
            version.CreatedByName,
            [.. version.Lines.OrderBy(line => line.SortOrder).Select(line => LineOf(line, models, categories))]
        );
    }

    private static RiderLineItem LineOf(
        RiderLine line,
        IReadOnlyDictionary<Guid, ModelSummary> models,
        IReadOnlyDictionary<Guid, CategorySummary> categories
    )
    {
        (string name, IReadOnlyList<string> path, bool isActive) = line switch
        {
            { ModelId: { } modelId } when models.TryGetValue(modelId, out ModelSummary? model) => (
                model.DisplayName,
                model.CategoryPath,
                model.IsActive
            ),
            { CategoryId: { } categoryId } when categories.TryGetValue(categoryId, out CategorySummary? category) => (
                category.Name,
                category.Path,
                category.IsActive
            ),
            _ => ("—", (IReadOnlyList<string>)[], false),
        };
        return new RiderLineItem(
            line.Id,
            line.LineKey,
            line.ModelId,
            line.CategoryId,
            name,
            path,
            isActive,
            line.Quantity,
            line.Flexibility,
            [
                .. line
                    .Equivalents.OrderBy(equivalent => equivalent.SortOrder)
                    .Select(equivalent =>
                        models.TryGetValue(equivalent.ModelId, out ModelSummary? model)
                            ? new RiderEquivalentItem(equivalent.ModelId, model.DisplayName, model.IsActive)
                            : new RiderEquivalentItem(equivalent.ModelId, "—", false)
                    ),
            ],
            line.Note
        );
    }
}
