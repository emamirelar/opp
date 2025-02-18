namespace UNOPS.PAO.Utilities.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;

public static class QueryExtensions
{

    public static bool NotDeleted<T>(this T query) where T : class, IDeletable
    {
        return !query.IsDeleted;
    }

    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> query) where T : class, IDeletable
    {
        return query.Where(a => !a.IsDeleted);
    }

    public static IEnumerable<T> NotDeleted<T>(this IEnumerable<T> query) where T : class, IDeletable
    {
        return query.Where(a => !a.IsDeleted);
    }

    public static IQueryable<Notification> ApplyFilters(this IQueryable<Notification> notifications, NotificationFilterModel? filter = null)
    {
        if (filter == null)
        {
            return notifications;
        }

        if (!string.IsNullOrEmpty(filter.SearchQuery))
        {
            notifications = notifications.Where(a => a.Headline.ToLower().Contains(filter.SearchQuery.ToLower()));
        }

        return notifications;
    }

    public static TSource SingleOrException<TSource>(this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate = null)
    {
        var item = predicate != null
            ? source.SingleOrDefault(predicate)
            : source.SingleOrDefault();

        if (item == null)
        {
            throw new BusinessException("Record not found.");
        }

        return item;
    }

    public static PaginationResponse<TSource> Paginate<TSource, TResult>(
        this IQueryable<TResult> query,
        Func<TResult, TSource> transform,
        PaginationRequest request)
    {
        var excludedRows = (request.PageIndex - 1) * request.PageSize;

        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }

        return new PaginationResponse<TSource>
        {
            TotalCount = query.Count(),
            Records = query
                .Skip(excludedRows)
                .Take(request.PageSize)
                .Select(transform)
                .ToList()
        };
    }

    public static async Task<PaginationResponse<TSource>> PaginateAsync<TSource, TResult>(
        this IQueryable<TResult> query,
        Func<TResult, TSource> transform,
        PaginationRequest request)
    {
        var excludedRows = (request.PageIndex - 1) * request.PageSize;

        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }

        var records = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginationResponse<TSource>
        {
            TotalCount = await query.CountAsync(),
            Records = records.Select(transform).ToList()
        };
    }

    public static IQueryable<TEntity> ApplyFilters<TEntity, TEntityFilterModel>(this IQueryable<TEntity> entity,
    TEntityFilterModel? filter)
    {
        if (filter == null)
        {
            return entity;
        }

        Type type = typeof(QueryExtensions);
        var filterMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(mi => mi.Name == nameof(ApplyFilters) && mi.ReturnType == typeof(IQueryable<TEntity>));

        if (filterMethod == null)
        {
            throw new NotImplementedException(
                $"No implementation for {nameof(ApplyFilters)} with parameter type {typeof(TEntity)}.");
        }

        return (IQueryable<TEntity>)filterMethod.Invoke(null, new object[] { entity, filter });
    }
}
