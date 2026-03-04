using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDomain.Specifications;

/// <summary>
/// Composite specification for Interactions with integrated RBAC filtering
/// This specification applies security filters at the database level for proper pagination
/// </summary>
public class InteractionRBACCompositeSpecification : GenericCompositeSpecification<UNOPSInteraction, IInteractionSearchFilter>
{
    private readonly ClaimsPrincipal _user;
    private readonly string _userOrgUnit;
    
    public InteractionRBACCompositeSpecification(
        IInteractionSearchFilter filter, 
        ClaimsPrincipal user,
        string? userOrgUnit = null) 
        : base(filter)
    {
        _user = user;
        _userOrgUnit = userOrgUnit ?? string.Empty;
        
        // Add standard includes for interactions
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
        AddInclude(i => i.InteractionPartners!);
        AddInclude("InteractionPartners.Partner");
        AddInclude(i => i.InteractionUsers!);
        
        // Apply security-based filtering
        ApplySecurityFilters();
        
        // Apply default sorting
        ApplyOrderByDescending(i => i.Date);
    }
    
    /// <summary>
    /// Applies security filters based on user roles and organization context
    /// This replaces the post-query RBAC filtering with pre-query filtering
    /// </summary>
    private void ApplySecurityFilters()
    {
        // If user is global admin, no additional filtering needed
        if (_user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return;
        }
        
        // Build security expression based on user roles and context
        Expression<Func<UNOPSInteraction, bool>>? securityExpression = null;
        
        // Get current user ID for ownership checks
        var userId = GetCurrentUserId();
        
        // Role-based access patterns with proper SQL generation
        if (_user.IsInRole("INTERACTION_MANAGER"))
        {
            // Can see all interactions in their org unit
            if (!string.IsNullOrEmpty(_userOrgUnit))
            {
                // Note: OrganizationUnitRelationships filtering moved to post-query processing
                securityExpression = i => (i.InteractionPartners ?? Enumerable.Empty<InteractionPartner>()).Any(ip =>
                    ip.Partner != null); // Org unit filtering will be done after manual loading
            }
            else
            {
                // If no org unit, can see interactions they created or are assigned to
                securityExpression = i => i.CreatedBy == userId ||
                                        (i.InteractionUsers ?? Enumerable.Empty<InteractionUser>()).Any(iu => iu.UserId == userId);
            }
        }
        else if (_user.IsInRole("INTERACTION_READ"))
        {
            // Can only see interactions they created or are explicitly assigned to
            securityExpression = i => i.CreatedBy == userId ||
                                    (i.InteractionUsers ?? Enumerable.Empty<InteractionUser>()).Any(iu => iu.UserId == userId);
        }
        else if (_user.IsInRole("PARTNER_MANAGER"))
        {
            // Can see interactions related to partners they manage in their org unit
            if (!string.IsNullOrEmpty(_userOrgUnit))
            {
                // Note: OrganizationUnitRelationships filtering moved to post-query processing
                securityExpression = i => (i.InteractionPartners ?? Enumerable.Empty<InteractionPartner>()).Any(ip =>
                    ip.Partner != null); // Org unit filtering will be done after manual loading
            }
            else
            {
                // If no org unit, can see interactions they created
                securityExpression = i => i.CreatedBy == userId;
            }
        }
        
        // Apply the security filter if one was built
        if (securityExpression != null)
        {
            // Combine with existing criteria using AND logic
            if (Criteria != null)
            {
                var combined = CombineExpressions(Criteria, securityExpression);
                ReplaceCriteria(combined);
            }
            else
            {
                ReplaceCriteria(securityExpression);
            }
        }
    }
    
    /// <summary>
    /// Gets the current user ID from claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = _user.FindFirst("user_id") ?? _user.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
    }
    
    /// <summary>
    /// Replaces the current criteria expression
    /// </summary>
    private void ReplaceCriteria(Expression<Func<UNOPSInteraction, bool>> newCriteria)
    {
        // Use reflection to set the protected Criteria property
        var criteriaProperty = typeof(BaseSpecification<UNOPSInteraction>)
            .GetProperty("Criteria", BindingFlags.NonPublic | BindingFlags.Instance);
        criteriaProperty?.SetValue(this, newCriteria);
    }
}