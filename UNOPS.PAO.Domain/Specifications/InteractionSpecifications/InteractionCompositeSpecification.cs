namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using System;
using System.Linq.Expressions;
using System.Text;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

/// <summary>
/// A composite specification that allows filtering interactions by multiple criteria
/// </summary>
public class InteractionCompositeSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for interactions
    /// </summary>
    /// <param name="contactId">Optional contact ID to filter by</param>
    /// <param name="type">Optional interaction type to filter by</param>
    /// <param name="fromDate">Optional start date to filter by</param>
    /// <param name="toDate">Optional end date to filter by</param>
    /// <param name="searchText">Optional text to search for in interaction data</param>
    public InteractionCompositeSpecification(
        int? contactId = null,
        InteractionType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchText = null)
        : base(BuildExpression(contactId, type, fromDate, toDate, searchText))
    {
        // Include the related contact
        AddInclude(i => i.Contact);
        
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
    }
    
    /// <summary>
    /// Builds the composite filter expression based on provided parameters
    /// </summary>
    private static Expression<Func<Interaction, bool>> BuildExpression(
        int? contactId,
        InteractionType? type,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchText)
    {
        // Start with a predicate that matches everything
        Expression<Func<Interaction, bool>> predicate = i => true;
        
        // Add contact filter if specified
        if (contactId.HasValue)
        {
            Expression<Func<Interaction, bool>> contactFilter = i => i.ContactId == contactId.Value;
            predicate = CombineExpressions(predicate, contactFilter);
        }
        
        // Add type filter if specified
        if (type.HasValue)
        {
            Expression<Func<Interaction, bool>> typeFilter = i => i.Type == type.Value;
            predicate = CombineExpressions(predicate, typeFilter);
        }
        
        // Add from date filter if specified
        if (fromDate.HasValue)
        {
            Expression<Func<Interaction, bool>> fromDateFilter = i => i.Date >= fromDate.Value;
            predicate = CombineExpressions(predicate, fromDateFilter);
        }
        
        // Add to date filter if specified
        if (toDate.HasValue)
        {
            Expression<Func<Interaction, bool>> toDateFilter = i => i.Date <= toDate.Value;
            predicate = CombineExpressions(predicate, toDateFilter);
        }
        
        // Add text search filter if specified
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Always perform case-insensitive search
            string lowerSearchText = searchText.ToLower();
            Expression<Func<Interaction, bool>> textFilter = i => 
                i.Data != null && Encoding.UTF8.GetString(i.Data).ToLower().Contains(lowerSearchText);
            predicate = CombineExpressions(predicate, textFilter);
        }
        
        return predicate;
    }
    
    /// <summary>
    /// Combines two expressions with an AND operator
    /// </summary>
    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        // If one of the expressions is a match-all expression (i => true), return the other
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
    private static bool IsMatchAllExpression<T>(Expression<Func<T, bool>> expr)
    {
        if (expr.Body is ConstantExpression constExpr)
        {
            return constExpr.Type == typeof(bool) && (bool)constExpr.Value;
        }
        return false;
    }
    
    /// <summary>
    /// Expression visitor that replaces parameters in an expression
    /// </summary>
    private class ReplaceParameterVisitor : ExpressionVisitor
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