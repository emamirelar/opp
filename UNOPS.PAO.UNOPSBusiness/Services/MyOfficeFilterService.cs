namespace UNOPS.PAO.UNOPSBusiness.Services;

using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;

/// <summary>
/// Service to handle MyOffice filtering logic for UNOPS entities
/// </summary>
public interface IMyOfficeFilterService
{
    Task<ISpecification<UNOPSPartner>> CreatePartnerSpecificationAsync(IPartnerSearchFilter filter, ClaimsPrincipal user);
    Task<ISpecification<UNOPSContact>> CreateContactSpecificationAsync(IContactSearchFilter filter, ClaimsPrincipal user);
    Task<ISpecification<Interaction>> CreateInteractionSpecificationAsync(IInteractionSearchFilter filter, ClaimsPrincipal user);
}

public class MyOfficeFilterService : IMyOfficeFilterService
{
    private readonly IBusinessSecurityService _securityService;
    private readonly ILogger<MyOfficeFilterService> _logger;

    public MyOfficeFilterService(
        IBusinessSecurityService securityService, 
        ILogger<MyOfficeFilterService> logger)
    {
        _securityService = securityService;
        _logger = logger;
    }

    public async Task<ISpecification<UNOPSPartner>> CreatePartnerSpecificationAsync(IPartnerSearchFilter filter, ClaimsPrincipal user)
    {
        _logger.LogInformation("Creating UNOPS partner specification with MyOffice support");
        
        if (filter.MyOfficeOnly)
        {
            var userOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            _logger.LogInformation("MyOfficeOnly requested, user org unit: {OrgUnit}", userOrgUnit);
            
            return new UNOPSPartnerCompositeWithMyOfficeSpecification(filter, userOrgUnit);
        }
        
        // Fallback to basic status specification
        return new UNOPSPartnerByStatusSpecification(filter.Status);
    }

    public async Task<ISpecification<UNOPSContact>> CreateContactSpecificationAsync(IContactSearchFilter filter, ClaimsPrincipal user)
    {
        _logger.LogInformation("Creating UNOPS contact specification with MyOffice support");
        
        if (filter.MyOfficeOnly)
        {
            var userOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            _logger.LogInformation("MyOfficeOnly requested, user org unit: {OrgUnit}", userOrgUnit);
            
            return new UNOPSContactCompositeWithMyOfficeSpecification(filter, userOrgUnit);
        }
        
        // Fallback to basic title specification
        return new UNOPSContactByTitleSpecification(filter.Title);
    }

    public async Task<ISpecification<Interaction>> CreateInteractionSpecificationAsync(IInteractionSearchFilter filter, ClaimsPrincipal user)
    {
        _logger.LogInformation("Creating interaction specification with MyOffice support");
        
        if (filter.MyOfficeOnly)
        {
            var userOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            _logger.LogInformation("MyOfficeOnly requested, user org unit: {OrgUnit}", userOrgUnit);
            
            return new InteractionCompositeWithMyOfficeSpecification(filter, userOrgUnit);
        }
        
        // Fallback to standard specification
        return new InteractionCompositeSpecification(filter);
    }
}