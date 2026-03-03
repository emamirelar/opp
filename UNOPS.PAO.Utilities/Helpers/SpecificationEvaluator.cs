namespace UNOPS.PAO.Utilities.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using UNOPS.PAO.Domain.Specifications;

public static class SpecificationEvaluator
{
    /// <summary>
    /// Apply a specification to an IQueryable
    /// </summary>
    /// <typeparam name="T">Type of entity</typeparam>
    /// <param name="inputQuery">The input query</param>
    /// <param name="specification">The specification to apply</param>
    /// <returns>A filtered and ordered query</returns>
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification) where T : class
    {
        var query = inputQuery;
        
        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }
        
        // Apply ordering
        IOrderedQueryable<T>? orderedQuery = null;
        if (specification.OrderBy != null)
        {
            orderedQuery = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            orderedQuery = query.OrderByDescending(specification.OrderByDescending);
        }
        
        // Apply additional ordering
        if (orderedQuery != null)
        {
            foreach (var orderExpression in specification.OrderByExpressions)
            {
                orderedQuery = orderExpression.Ascending 
                    ? orderedQuery.ThenBy(orderExpression.KeySelector) 
                    : orderedQuery.ThenByDescending(orderExpression.KeySelector);
            }
            
            query = orderedQuery;
        }
        
        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }
        
        // Apply includes
        query = specification.Includes.Aggregate(query, 
            (current, include) => current.Include(include));
        
        // Apply string includes
        query = specification.IncludeStrings.Aggregate(query, 
            (current, include) => current.Include(include));
        
        // Apply complex includes
        query = specification.IncludeExpressions.Aggregate(query,
            (current, include) => include(current));
        
        return query;
    }
} 