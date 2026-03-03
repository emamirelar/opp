namespace UNOPS.PAO.Utilities.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Interfaces;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Notifications;
using UNOPS.PAO.Models.Shared;
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
        Expression<Func<TSource, bool>>? predicate = null)
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
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;

        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }

        // Get the total count first
        var totalCount = query.Count();
        
        // Materialize the query results to avoid concurrent database operations
        var queryResults = query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToList();

        return new PaginationResponse<TSource>
        {
            TotalCount = totalCount,
            Records = queryResults.Select(transform).ToList(),
            PageIndex = pageIndex,
            PageSize = request.PageSize,
            TotalPages = request.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / request.PageSize) : 0
        };
    }

    public static async Task<PaginationResponse<TSource>> PaginateAsync<TSource, TResult>(
        this IQueryable<TResult> query,
        Func<TResult, TSource> transform,
        PaginationRequest request)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;

        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }

        // Get the total count first
        var totalCount = await query.CountAsync();
        
        // Materialize the query results to avoid concurrent database operations
        var records = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginationResponse<TSource>
        {
            TotalCount = totalCount,
            Records = records.Select(transform).ToList(),
            PageIndex = pageIndex,
            PageSize = request.PageSize,
            TotalPages = request.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / request.PageSize) : 0
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
            .SingleOrDefault(mi => mi.Name == nameof(ApplyFilters) && mi.ReturnType == typeof(IQueryable<TEntity>));

        if (filterMethod == null)
        {
            throw new NotImplementedException(
                $"No implementation for {nameof(ApplyFilters)} with parameter type {typeof(TEntity)}.");
        }

        var result = filterMethod.Invoke(null, new object[] { entity, filter! });
        return (IQueryable<TEntity>)result!;
    }

    public static IQueryable<TEntity> ApplySpecification<TEntity>(this IQueryable<TEntity> query, 
        ISpecification<TEntity> specification) where TEntity : class
    {
        return SpecificationEvaluator.GetQuery(query, specification);
    }

    public static PaginationResponse<TSource> PaginateWithSpecification<TSource, TEntity>(
        this IQueryable<TEntity> query,
        Func<TEntity, TSource> transform,
        SpecificationPaginationRequest<TEntity> request) where TEntity : class
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        var filteredQuery = query.ApplySpecification(request.Specification);
        
        if (request.OrderBy != null)
        {
            filteredQuery = filteredQuery.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the total count first
        var totalCount = filteredQuery.Count();
        
        // Materialize the query results to avoid concurrent database operations
        var queryResults = filteredQuery
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToList();
        
        return new PaginationResponse<TSource>
        {
            TotalCount = totalCount,
            Records = queryResults.Select(transform).ToList(),
            PageIndex = pageIndex,
            PageSize = request.PageSize,
            TotalPages = request.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / request.PageSize) : 0
        };
    }

    public static async Task<PaginationResponse<TSource>> PaginateWithSpecificationAsync<TSource, TEntity>(
        this IQueryable<TEntity> query,
        Func<TEntity, TSource> transform,
        SpecificationPaginationRequest<TEntity> request) where TEntity : class
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        var filteredQuery = query.ApplySpecification(request.Specification);
        
        if (request.OrderBy != null)
        {
            filteredQuery = filteredQuery.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the total count first
        var totalCount = await filteredQuery.CountAsync();
        
        // Materialize the query results to avoid concurrent database operations
        var records = await filteredQuery
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();
        
        return new PaginationResponse<TSource>
        {
            TotalCount = totalCount,
            Records = records.Select(transform).ToList(),
            PageIndex = pageIndex,
            PageSize = request.PageSize,
            TotalPages = request.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / request.PageSize) : 0
        };
    }

}
