namespace UNOPS.PAO.Utilities.Helpers;

using System.Linq.Expressions;

public static class EnumerableExtensions
{
    public static void AddRange<T>(this ICollection<T> toList, IEnumerable<T> list)
    {
        foreach (var item in list)
        {
            toList.Add(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    /// <summary>
    ///     Sorts the elements of a sequence according to a key and the sort order.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="query" />.</typeparam>
    /// <param name="query">A sequence of values to order.</param>
    /// <param name="key">Name of the property of <see cref="TSource" /> by which to sort the elements.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>
    ///     An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key and sort
    ///     order.
    /// </returns>
    public static IQueryable<TSource> OrderByColumnName<TSource>(this IQueryable<TSource> query, string key, bool ascending = true)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return query;
        }

        try
        {
            var lambda = (dynamic)CreateExpression(typeof(TSource), key);

            return ascending
                ? Queryable.OrderBy(query, lambda)
                : Queryable.OrderByDescending(query, lambda);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("is not a member of type"))
        {
            // Invalid property name - return query without ordering
            // Log this in production to identify problematic property names
            System.Diagnostics.Debug.WriteLine($"OrderByColumnName: Invalid property '{key}' for type '{typeof(TSource).Name}'. Skipping ordering.");
            return query;
        }
        catch (Exception)
        {
            // Any other error - return query without ordering to prevent crashes
            return query;
        }
    }

    private static LambdaExpression CreateExpression(Type type, string propertyName)
    {
        var param = Expression.Parameter(type, "x");

        Expression body = param;
        foreach (var member in propertyName.Split('.'))
        {
            body = Expression.PropertyOrField(body, member);
        }

        return Expression.Lambda(body, param);
    }
}