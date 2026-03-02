namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// A composite specification that allows filtering partners by multiple criteria
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class PartnerCompositeClassicSearchSpecification : BaseSpecification<Partner>
{
    private readonly int? _organizationHierarchyId;
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for partners
    /// </summary>
    /// <param name="id">Optional partner ID to filter by</param>
    /// <param name="name">Optional partner name to filter by</param>
    /// <param name="status">Optional status to filter by</param>
    /// <param name="newEngagement">Optional new engagement status to filter by</param>
    /// <param name="phone">Optional phone number to filter by</param>
    /// <param name="website">Optional website to filter by</param>
    /// <param name="shortName">Optional short name to filter by</param>
    /// <param name="organizationHierarchyId">Optional organization hierarchy ID to filter by</param>
    /// <param name="partnerCategoryId">Optional partner category ID to filter by</param>
    /// <param name="addressCity">Optional city to filter by</param>
    /// <param name="addressStateProvince">Optional state/province to filter by</param>
    /// <param name="addressPostalCode">Optional postal code to filter by</param>
    /// <param name="addressCountry">Optional country to filter by</param>
    /// <param name="searchText">Optional text to search for in partner name, short name or phone</param>
    public PartnerCompositeClassicSearchSpecification(
        int? id = null,
        string? name = null,
        string? status = null,
        string? newEngagement = null,
        string? phone = null,
        string? website = null,
        string? shortName = null,
        int? organizationHierarchyId = null,
        int? partnerCategoryId = null,
        string? addressCity = null,
        string? addressStateProvince = null,
        string? addressPostalCode = null,
        string? addressCountry = null,
        string? searchText = null)
        : base(BuildExpression(id, name, status, newEngagement, phone, website, shortName, 
                              organizationHierarchyId, partnerCategoryId, addressCity, addressStateProvince, 
                              addressPostalCode, addressCountry, searchText))
    {
        _organizationHierarchyId = organizationHierarchyId;
        // Include related entities
        
        // Default ordering is by partner description
        ApplyOrderBy(p => p.Name);
    }
    
    /// <summary>
    /// Builds the composite filter expression based on provided parameters
    /// </summary>
    private static Expression<Func<Partner, bool>> BuildExpression(
        int? id,
        string? name,
        string? status,
        string? newEngagement,
        string? phone,
        string? website,
        string? shortName,
        int? organizationHierarchyId,
        int? partnerCategoryId,
        string? addressCity,
        string? addressStateProvince,
        string? addressPostalCode,
        string? addressCountry,
        string? searchText)
    {
        // Start with a predicate that matches everything
        Expression<Func<Partner, bool>> predicate = p => true;
        
        // Add ID filter if specified
        if (id.HasValue)
        {
            Expression<Func<Partner, bool>> idFilter = p => p.Id == id.Value;
            predicate = CombineExpressions(predicate, idFilter);
        }
        
        // Add name filter if specified (using PartnerDescription)
        if (!string.IsNullOrWhiteSpace(name))
        {
            Expression<Func<Partner, bool>> nameFilter = p => p.Name.ToLower().Contains(name.ToLower());
            predicate = CombineExpressions(predicate, nameFilter);
        }
        
        // Add status filter if specified (using SystemStatus)
        if (!string.IsNullOrWhiteSpace(status))
        {
            Expression<Func<Partner, bool>> statusFilter = p => p.Status.ToString() == status;
            predicate = CombineExpressions(predicate, statusFilter);
        }
        
        // Add new engagement filter if specified (using CanCreateNewOpportunities)
        if (!string.IsNullOrWhiteSpace(newEngagement))
        {
            Expression<Func<Partner, bool>> newEngagementFilter = p => newEngagement.ToLower() == "yes" ? p.CanCreateNewOpportunities : !p.CanCreateNewOpportunities;
            predicate = CombineExpressions(predicate, newEngagementFilter);
        }
        
        // Add short name filter if specified (using PartnerShortDescription)
        if (!string.IsNullOrWhiteSpace(shortName))
        {
            Expression<Func<Partner, bool>> shortNameFilter = p => p.PartnerShortDescription != null && p.PartnerShortDescription.ToLower().Contains(shortName.ToLower());
            predicate = CombineExpressions(predicate, shortNameFilter);
        }
        
        // Add organization hierarchy filter if specified
        if (organizationHierarchyId.HasValue)
        {
            // Note: OrganizationUnitRelationships filtering moved to manual join method
            Expression<Func<Partner, bool>> organizationHierarchyFilter = p => true;
            predicate = CombineExpressions(predicate, organizationHierarchyFilter);
        }
        
        // Add text search filter if specified (updated to use new fields)
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Always perform case-insensitive search
            string lowerSearchText = searchText.ToLower();
            Expression<Func<Partner, bool>> textFilter = p => 
                (p.Name != null && p.Name.ToLower().Contains(lowerSearchText)) ||
                (p.PartnerShortDescription != null && p.PartnerShortDescription.ToLower().Contains(lowerSearchText)) ||
                (p.PartnerLongDescription != null && p.PartnerLongDescription.ToLower().Contains(lowerSearchText));
            predicate = CombineExpressions(predicate, textFilter);
        }
        
        return predicate;
    }
    
    /// <summary>
    /// Apply manual join filtering for organization hierarchy if specified
    /// This should be called by the repository/manager when applying the specification
    /// </summary>
    public IQueryable<Partner> ApplyOrgUnitFilter(IQueryable<Partner> query, DbContext context)
    {
        if (!_organizationHierarchyId.HasValue)
        {
            return query;
        }

        // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
        var validPartnerIds = context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                orgRel.OrganizationHierarchyId == _organizationHierarchyId.Value)
            .Select(orgRel => orgRel.EntityId)
            .ToList(); // Materialize the IDs first

        // Now filter the partners using the materialized IDs
        return query.Where(partner => validPartnerIds.Contains(partner.Id));
    }
    
    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        // If the first expression is just "x => true", return the second expression
        if (IsMatchAllExpression(expr1))
        {
            return expr2;
        }
        
        // Create a parameter for the combined expression
        var parameter = Expression.Parameter(typeof(T), "x");
        
        // Replace the parameter in both expressions with our new parameter
        var visitor1 = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
        var visitor2 = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
        
        var body1 = visitor1.Visit(expr1.Body);
        var body2 = visitor2.Visit(expr2.Body);
        
        // Combine the two expression bodies with AND
        var combinedBody = Expression.AndAlso(body1, body2);
        
        // Create a new lambda expression with the combined body
        return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
    }
    
    private static bool IsMatchAllExpression<T>(Expression<Func<T, bool>> expr)
    {
        if (expr.Body is ConstantExpression constantExpr)
        {
            return constantExpr.Type == typeof(bool) && constantExpr.Value != null && (bool)constantExpr.Value;
        }
        
        return false;
    }
    
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