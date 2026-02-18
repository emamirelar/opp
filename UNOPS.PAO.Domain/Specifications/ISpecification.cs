namespace UNOPS.PAO.Domain.Specifications;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// Interface for the Specification Pattern. Specifications encapsulate query logic.
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Main criteria expression used to filter entities
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }
    
    /// <summary>
    /// Collection of include expressions for eager loading related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Collection of string-based include paths for eager loading related entities
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Collection of include expressions for eager loading related entities with additional ThenInclude statements
    /// </summary>
    List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeExpressions { get; }
    
    /// <summary>
    /// Expression to order entities by
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Expression to order entities by descending
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    /// <summary>
    /// Additional ordering expressions
    /// </summary>
    List<(Expression<Func<T, object>> KeySelector, bool Ascending)> OrderByExpressions { get; }
    
    /// <summary>
    /// Number of entities to skip for pagination
    /// </summary>
    int Skip { get; }
    
    /// <summary>
    /// Number of entities to take for pagination
    /// </summary>
    int Take { get; }
    
    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
} 