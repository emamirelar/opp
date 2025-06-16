namespace UNOPS.PAO.Domain.Specifications;

using System.Linq.Expressions;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Infrastructure;
using System.Diagnostics;

/// <summary>
/// A composite specification that supports user context for MyOfficeOnly filtering
/// This is a wrapper that combines base filtering with organizational unit filtering
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class UserContextCompositeSpecification<TEntity> : BaseCompositeSpecification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Creates a composite specification with user context support
    /// </summary>
    /// <param name="baseSpecification">The base specification for standard filtering</param>
    /// <param name="myOfficeSpecification">The MyOffice specification for organizational filtering</param>
    /// <param name="applyMyOffice">Whether to apply the MyOffice filter</param>
    public UserContextCompositeSpecification(
        ISpecification<TEntity> baseSpecification, 
        ISpecification<TEntity>? myOfficeSpecification = null, 
        bool applyMyOffice = false)
        : base(BuildCombinedExpression(baseSpecification, myOfficeSpecification, applyMyOffice))
    {
        // Copy includes from base specification
        if (baseSpecification.Includes.Any())
        {
            foreach (var include in baseSpecification.Includes)
            {
                AddInclude(include);
            }
        }

        // Copy includes from MyOffice specification if applicable
        if (myOfficeSpecification != null && applyMyOffice && myOfficeSpecification.Includes.Any())
        {
            foreach (var include in myOfficeSpecification.Includes)
            {
                AddInclude(include);
            }
        }

        // Copy ordering from base specification
        if (baseSpecification.OrderBy != null)
        {
            ApplyOrderBy(baseSpecification.OrderBy);
        }
        if (baseSpecification.OrderByDescending != null)
        {
            ApplyOrderByDescending(baseSpecification.OrderByDescending);
        }
    }

    private static Expression<Func<TEntity, bool>> BuildCombinedExpression(
        ISpecification<TEntity> baseSpecification, 
        ISpecification<TEntity>? myOfficeSpecification, 
        bool applyMyOffice)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var expressions = new List<Expression>();

        // Add base specification criteria
        if (baseSpecification.Criteria != null)
        {
            var baseBody = ExpressionParameterReplacer.Replace(
                baseSpecification.Criteria.Body, 
                baseSpecification.Criteria.Parameters[0], 
                parameter);
            expressions.Add(baseBody);
            Debug.WriteLine("Added base specification expression");
        }

        // Add MyOffice specification criteria if applicable
        if (applyMyOffice && myOfficeSpecification?.Criteria != null)
        {
            var myOfficeBody = ExpressionParameterReplacer.Replace(
                myOfficeSpecification.Criteria.Body, 
                myOfficeSpecification.Criteria.Parameters[0], 
                parameter);
            expressions.Add(myOfficeBody);
            Debug.WriteLine("Added MyOffice specification expression");
        }

        if (!expressions.Any())
        {
            Debug.WriteLine("No valid expressions, returning true");
            return x => true;
        }

        // Combine all expressions with AND
        Expression finalExpression = expressions[0];
        for (int i = 1; i < expressions.Count; i++)
        {
            finalExpression = Expression.AndAlso(finalExpression, expressions[i]);
        }

        Debug.WriteLine($"Combined {expressions.Count} expressions with user context");
        return Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);
    }
}

/// <summary>
/// Helper class to replace expression parameters
/// </summary>
internal class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    private ExpressionParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    public static Expression Replace(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        return new ExpressionParameterReplacer(oldParameter, newParameter).Visit(expression);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}