using System.Linq.Expressions;
using FestOS.BuildingBlocks.Application.Paging;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The columns a list may be sorted by, under the names its <c>sort</c> parameter uses (api §6.2). The
/// validator allows only <see cref="Names"/>, so user input never names a column.
/// </summary>
/// <typeparam name="T">The rows being sorted.</typeparam>
public sealed class SortKeys<T>
{
    private readonly Dictionary<string, LambdaExpression> _keys = new(StringComparer.Ordinal);

    /// <summary>The names the <c>sort</c> parameter may use.</summary>
    public IReadOnlyCollection<string> Names => _keys.Keys;

    /// <summary>Adds a sortable column.</summary>
    public SortKeys<T> Add<TKey>(string name, Expression<Func<T, TKey>> key)
    {
        _keys.Add(name, key);
        return this;
    }

    /// <summary>
    /// Orders the rows by the requested fields, then by the identifier, so rows with equal values keep
    /// their order from page to page.
    /// </summary>
    public IOrderedQueryable<T> Apply<TId>(IQueryable<T> rows, SortSpec sort, Expression<Func<T, TId>> identifier)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(sort);

        IQueryable<T> ordered = rows;
        bool isFirst = true;
        foreach (SortField field in sort.Fields)
        {
            ordered = OrderBy(ordered, _keys[field.Name], field.Descending, isFirst);
            isFirst = false;
        }

        return (IOrderedQueryable<T>)OrderBy(ordered, identifier, descending: false, isFirst);
    }

    private static IQueryable<T> OrderBy(IQueryable<T> rows, LambdaExpression key, bool descending, bool isFirst)
    {
        string method = (isFirst, descending) switch
        {
            (true, false) => nameof(Queryable.OrderBy),
            (true, true) => nameof(Queryable.OrderByDescending),
            (false, false) => nameof(Queryable.ThenBy),
            (false, true) => nameof(Queryable.ThenByDescending),
        };
        return rows.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(Queryable),
                method,
                [typeof(T), key.ReturnType],
                rows.Expression,
                Expression.Quote(key)
            )
        );
    }
}
