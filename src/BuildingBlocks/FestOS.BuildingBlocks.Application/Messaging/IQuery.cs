namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// A request to read state without changing it, handled by exactly one
/// <see cref="IQueryHandler{TQuery, TResult}"/>. Names end with <c>Query</c> (AT-07).
/// </summary>
/// <typeparam name="TResult">The data returned.</typeparam>
public interface IQuery<TResult>;
