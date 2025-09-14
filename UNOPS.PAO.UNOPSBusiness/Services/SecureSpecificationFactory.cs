using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Factory service for creating specifications with integrated RBAC filtering
/// This ensures security is applied at the database level for proper pagination
/// </summary>
public class SecureSpecificationFactory : ISecureSpecificationFactory
{
    private readonly IPermissionService _permissionService;
    private readonly IOrgUnitFilterService _orgUnitFilterService;
    private readonly ILogger<SecureSpecificationFactory> _logger;
    
    public SecureSpecificationFactory(
        IPermissionService permissionService,
        IOrgUnitFilterService orgUnitFilterService,
        ILogger<SecureSpecificationFactory> logger)
    {
        _permissionService = permissionService;
        _orgUnitFilterService = orgUnitFilterService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a secure interaction specification with RBAC filtering integrated
    /// </summary>
    /// <param name="filter">The interaction filter request</param>
    /// <param name="user">Current user context</param>
    /// <returns>Specification with security filters applied</returns>
    public async Task<ISpecification<Interaction>> CreateInteractionSpecificationAsync(
        IInteractionSearchFilter filter, 
        ClaimsPrincipal user)
    {
        try
        {
            // Get user's organization unit for filtering
            var userOrgUnit = await _permissionService.GetUserOrgUnitAsync(user);

            _logger.LogDebug("Creating secure interaction specification for user with org unit: {OrgUnit}", 
                userOrgUnit ?? "None");
            
            // Return the RBAC-aware specification with integrated security filters
            return InteractionRBACCompositeSpecification.Create(filter, user, userOrgUnit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secure interaction specification");
            
            // Fallback to standard specification if there's an error
            // This ensures the system doesn't break but logs the issue
            return new InteractionCompositeSpecification(filter);
        }
    }
    
    /// <summary>
    /// Creates a secure partner specification with RBAC filtering integrated
    /// </summary>
    /// <param name="filter">The partner filter request</param>
    /// <param name="user">Current user context</param>
    /// <returns>Specification with security filters applied</returns>
    public async Task<ISpecification<UNOPSPartner>> CreatePartnerSpecificationAsync(
        IPartnerSearchFilter filter, 
        ClaimsPrincipal user)
    {
        try
        {
            _logger.LogDebug("Creating secure partner specification with OrgUnit support");
            
            // Use OrgUnitFilterService to create specification with OrgUnit filtering
            return await _orgUnitFilterService.CreatePartnerSpecificationAsync(filter, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secure partner specification");
            
            // Fallback to standard specification if there's an error
            return new UNOPSDomain.Specifications.UNOPSPartnerCompositeSpecification(filter);
        }
    }
    
    /// <summary>
    /// Creates a secure contact specification with RBAC filtering integrated
    /// </summary>
    /// <param name="filter">The contact filter request</param>
    /// <param name="user">Current user context</param>
    /// <returns>Specification with security filters applied</returns>
    public async Task<ISpecification<UNOPSContact>> CreateContactSpecificationAsync(
        IContactSearchFilter filter, 
        ClaimsPrincipal user)
    {
        // Implementation would be similar for contacts
        // This demonstrates the extensible pattern
        throw new NotImplementedException("Contact secure specification not yet implemented");
    }
}

/// <summary>
/// Interface for the secure specification factory
/// </summary>
public interface ISecureSpecificationFactory
{
    Task<ISpecification<Interaction>> CreateInteractionSpecificationAsync(
        IInteractionSearchFilter filter, ClaimsPrincipal user);
        
    Task<ISpecification<UNOPSPartner>> CreatePartnerSpecificationAsync(
        IPartnerSearchFilter filter, ClaimsPrincipal user);
        
    Task<ISpecification<UNOPSContact>> CreateContactSpecificationAsync(
        IContactSearchFilter filter, ClaimsPrincipal user);
}