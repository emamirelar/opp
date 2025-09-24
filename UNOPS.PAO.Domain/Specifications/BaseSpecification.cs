namespace UNOPS.PAO.Domain.Specifications;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// Base implementation of ISpecification that can be inherited to create specific query specifications
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Constructor with criteria expression
    /// </summary>
    /// <param name="criteria">The filter criteria</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }
    
    /// <summary>
    /// Default constructor with criteria that matches all entities
    /// </summary>
    protected BaseSpecification()
    {
        Criteria = _ => true;
    }
    
    /// <inheritdoc />
    public Expression<Func<T, bool>> Criteria { get; }
    
    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    
    /// <inheritdoc />
    public List<string> IncludeStrings { get; } = new();
    
    /// <inheritdoc />
    public List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeExpressions { get; } = new();
    
    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    
    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    
    /// <inheritdoc />
    public List<(Expression<Func<T, object>> KeySelector, bool Ascending)> OrderByExpressions { get; } = new();
    
    /// <inheritdoc />
    public int Skip { get; private set; }
    
    /// <inheritdoc />
    public int Take { get; private set; }
    
    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }
    
    /// <summary>
    /// Add an include expression to eager load a related entity
    /// </summary>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    
    /// <summary>
    /// Add a string-based include path to eager load a related entity
    /// </summary>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }
    
    /// <summary>
    /// Add an include expression with ThenInclude support
    /// </summary>
    protected virtual void AddInclude(Func<IQueryable<T>, IIncludableQueryable<T, object>> includeExpression)
    {
        IncludeExpressions.Add(includeExpression);
    }
    
    /// <summary>
    /// Apply paging parameters
    /// </summary>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
    
    /// <summary>
    /// Apply an order by expression
    /// </summary>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
    
    /// <summary>
    /// Apply an order by descending expression
    /// </summary>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }
    
    /// <summary>
    /// Add an additional ordering expression
    /// </summary>
    protected virtual void AddOrderBy(Expression<Func<T, object>> orderByExpression, bool ascending = true)
    {
        OrderByExpressions.Add((orderByExpression, ascending));
    }
} 