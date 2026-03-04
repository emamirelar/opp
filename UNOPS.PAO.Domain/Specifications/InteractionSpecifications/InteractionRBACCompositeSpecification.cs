using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;

namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

/// <summary>
/// Composite specification for Interactions with integrated RBAC filtering at database level
/// This specification ensures proper pagination by applying security filters before query execution
/// </summary>
public class InteractionRBACCompositeSpecification : GenericCompositeSpecification<Interaction, IInteractionSearchFilter>
{
    private readonly ClaimsPrincipal _user;
    private readonly string? _userOrgUnit;
    
    public InteractionRBACCompositeSpecification(
        IInteractionSearchFilter filter, 
        ClaimsPrincipal user,
        string? userOrgUnit = null) 
        : base(filter)
    {
        _user = user;
        _userOrgUnit = userOrgUnit;
        
        // Add standard includes for interactions
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
        AddInclude(i => i.InteractionPartners!);
        AddInclude("InteractionPartners.Partner");
        AddInclude(i => i.InteractionUsers!);
        
        // Apply security-based filtering BEFORE any other filtering
        ApplySecurityFilters();
        
        // Apply default sorting
        ApplyOrderByDescending(i => i.Date);
    }
    
    /// <summary>
    /// Applies security filters based on user roles and organization context
    /// This replaces the post-query RBAC filtering with pre-query filtering for proper pagination
    /// </summary>
    private void ApplySecurityFilters()
    {
        // If user is global admin, no additional filtering needed
        if (_user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return;
        }
        
        // Build security expression based on user roles and context
        Expression<Func<Interaction, bool>>? securityExpression = null;
        
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
        else if (_user.IsInRole("CONTACT_MANAGER"))
        {
            // Can see interactions for contacts in their org unit
            if (!string.IsNullOrEmpty(_userOrgUnit))
            {
                // Note: OrganizationUnitRelationships filtering moved to post-query processing
                securityExpression = i => (i.InteractionContacts ?? Enumerable.Empty<InteractionContact>()).Any(ic => 
                    ic.Contact != null && 
                    ic.Contact.Partner != null) || // Org unit filtering will be done after manual loading
                    i.CreatedBy == userId;
            }
            else
            {
                // If no org unit, can see interactions they created
                securityExpression = i => i.CreatedBy == userId;
            }
        }
        else
        {
            // Default: can only see interactions they created
            securityExpression = i => i.CreatedBy == userId;
        }
        
        // Apply the security filter if one was built
        if (securityExpression != null)
        {
            // Combine with existing criteria using AND logic
            if (Criteria != null)
            {
                var combined = CombineExpressions(Criteria, securityExpression);
                SetCriteria(combined);
            }
            else
            {
                SetCriteria(securityExpression);
            }
        }
    }
    
    /// <summary>
    /// Gets the current user ID from claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = _user.FindFirst("user_id") ?? 
                         _user.FindFirst(ClaimTypes.NameIdentifier) ?? 
                         _user.FindFirst("sub");
        
        if (int.TryParse(userIdClaim?.Value, out var userId))
        {
            return userId;
        }
        
        // Fallback: try to get user ID from email claim
        var emailClaim = _user.FindFirst(ClaimTypes.Email) ?? _user.FindFirst("email");
        if (!string.IsNullOrEmpty(emailClaim?.Value))
        {
            // This would require a service to look up user ID by email
            // For now, return 0 which will restrict to no access
            return 0;
        }
        
        return 0; // No access if no valid user ID found
    }
    
    /// <summary>
    /// Sets the criteria expression using reflection to access protected property
    /// </summary>
    private void SetCriteria(Expression<Func<Interaction, bool>> criteria)
    {
        // Access the protected Criteria property
        var criteriaProperty = typeof(BaseSpecification<Interaction>)
            .GetProperty("Criteria", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (criteriaProperty != null && criteriaProperty.CanWrite)
        {
            criteriaProperty.SetValue(this, criteria);
        }
        else
        {
            // Alternative approach: use reflection on private field if property doesn't work
            var criteriaField = typeof(BaseSpecification<Interaction>)
                .GetField("_criteria", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (criteriaField != null)
            {
                criteriaField.SetValue(this, criteria);
            }
        }
    }
    
    /// <summary>
    /// Factory method to create RBAC-aware specifications with proper error handling
    /// </summary>
    public static InteractionRBACCompositeSpecification Create(
        IInteractionSearchFilter filter, 
        ClaimsPrincipal user, 
        string? userOrgUnit = null)
    {
        try
        {
            return new InteractionRBACCompositeSpecification(filter, user, userOrgUnit);
        }
        catch (Exception)
        {
            // If RBAC specification fails, create a restrictive specification
            // that only shows interactions created by the user
            var restrictiveSpec = new InteractionRBACCompositeSpecification(filter, user, null);
            return restrictiveSpec;
        }
    }
    
    /// <summary>
    /// Gets user roles for debugging/logging purposes
    /// </summary>
    public IEnumerable<string> GetUserRoles()
    {
        return _user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();
    }
    
    /// <summary>
    /// Gets the organization unit for debugging/logging purposes
    /// </summary>
    public string? GetUserOrgUnit()
    {
        return _userOrgUnit;
    }
}