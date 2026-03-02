namespace UNOPS.PAO.Domain.Specifications;

using System;
using System.Linq.Expressions;

/// <summary>
/// Base class for composite specifications that provides common functionality
/// </summary>
public abstract class BaseCompositeSpecification<T> : BaseSpecification<T>
{
    protected BaseCompositeSpecification(Expression<Func<T, bool>> criteria) : base(criteria)
    {
    }

    /// <summary>
    /// Combines two expressions with an AND operator
    /// </summary>
    protected static Expression<Func<T, bool>> CombineExpressions(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        // If one of the expressions is a match-all expression (x => true), return the other
        if (IsMatchAllExpression(expr1))
            return expr2;
        if (IsMatchAllExpression(expr2))
            return expr1;
            
        // Create a parameter for the combined expression
        var parameter = Expression.Parameter(typeof(T), "x");
        
        // Replace the parameters in the expressions with our new parameter
        var leftVisitor = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);
        
        var rightVisitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);
        
        // Combine the expressions with an AND operator
        var body = Expression.AndAlso(left, right);
        
        // Create and return the combined expression
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
    
    /// <summary>
    /// Checks if the expression is a match-all expression (x => true)
    /// </summary>
    protected static bool IsMatchAllExpression(Expression<Func<T, bool>> expr)
    {
        if (expr.Body is ConstantExpression constExpr)
        {
            return constExpr.Type == typeof(bool) && constExpr.Value != null && (bool)constExpr.Value;
        }
        return false;
    }
    
    /// <summary>
    /// Expression visitor that replaces parameters in an expression
    /// </summary>
    protected class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;
        
        public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }
        
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
} 