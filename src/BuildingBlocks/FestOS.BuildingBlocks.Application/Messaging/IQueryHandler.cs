namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>Handles one query type; wrapped in the logging and validation decorators.</summary>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>Handles the query.</summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
