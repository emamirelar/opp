using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using System;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Presentation;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Specifications;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Entities;

// using UNOPS.PAO.UNOPSBusiness.Authorization;
// using UNOPS.PAO.UNOPSBusiness.Attributes;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerController : BaseController
{
    private readonly IPartnerManager _manager;
    private readonly IOrgUnitFilterService _orgUnitFilterService;

    public PartnerController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerController> logger,
        IOrgUnitFilterService orgUnitFilterService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.PartnerManager;
        _orgUnitFilterService = orgUnitFilterService;
    }

    [HttpPost(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "create")]
    public async Task<IActionResult> Create([FromBody] PartnerRequest req)
    {
        var result = await _manager.CreatePartnerAsync(User, req);
        if (result == null)
        {
            throw new BusinessException("Failed to create partner");
        }
        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetAll(
        [FromQuery] PartnerFilterRequest request, 
        [FromQuery] bool advancedSearch = false, 
        [FromQuery] string? searchCriteria = null,
        [FromQuery] string? searchText = null)
    {
        
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        // Handle search using the new helper methods
        return await HandleSearchOperationAsync(async () =>
        {
            if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
            {
                // For advanced search, we need to parse the search criteria and combine with org unit filter
                // Since SearchControllerHelper expects a synchronous factory, we'll create the spec inline
                return await SearchControllerHelper.ProcessAdvancedSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
                    searchCriteria, searchText ?? request.SearchText, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending, 
                    request, // Use request as pagination request since PartnerFilterRequest extends PaginationRequest
                    "Partner",
                    filterRequest => new PartnerCompositeSpecification(filterRequest),
                    async (userId, spec, pagination) => {
                        // If OrgUnitId is specified, use the OrgUnitFilterService to create a proper specification
                        if (pagination is PartnerFilterRequest partnerPagination && partnerPagination.OrgUnitId.HasValue)
                        {
                            var orgUnitSpec = await _orgUnitFilterService.CreatePartnerSpecificationAsync(partnerPagination, User);
                            var adaptedSpec = new PartnerSpecificationAdapter(orgUnitSpec);
                            return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, adaptedSpec, partnerPagination);
                        }
                        return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
                    },
                    CurrentUserId, _logger);
            }
            
            // Handle simple text search
            if (!string.IsNullOrWhiteSpace(searchText) || !string.IsNullOrWhiteSpace(request.SearchText))
            {
                var textToSearch = searchText ?? request.SearchText;
                return await SearchControllerHelper.ProcessSimpleTextSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
                    textToSearch!, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending,
                    request,
                    "Partner",
                    filterRequest => new PartnerCompositeSpecification(filterRequest),
                    async (userId, spec, pagination) => {
                        // If OrgUnitId is specified, use the OrgUnitFilterService to create a proper specification
                        if (pagination is PartnerFilterRequest partnerPagination && partnerPagination.OrgUnitId.HasValue)
                        {
                            var orgUnitSpec = await _orgUnitFilterService.CreatePartnerSpecificationAsync(partnerPagination, User);
                            var adaptedSpec = new PartnerSpecificationAdapter(orgUnitSpec);
                            return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, adaptedSpec, partnerPagination);
                        }
                        return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
                    },
                    CurrentUserId, _logger);
            }
            
            // For no search parameters, return all partners with pagination
            _logger.LogInformation("Retrieving all partners with pagination. OrgUnitId filter: {OrgUnitId}", request.OrgUnitId);
            
            // Use OrgUnitFilterService to create the appropriate specification
            var unosPartnerSpec = await _orgUnitFilterService.CreatePartnerSpecificationAsync(request, User);
            var specification = new PartnerSpecificationAdapter(unosPartnerSpec);
            
            var result = await _manager.GetPartnersWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<PartnerModel>)result;
        }, "partner search");
    }

    [HttpGet(APIDictionary.Partner + "/{id}")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<IActionResult> Get(int id)
    {
        var partner = await _manager.GetPartnerAsync(User, id);
        if (partner == null)
        {
            return NotFound();
        }

        // Return partner data directly using JsonResult to avoid the wrapper metadata
        return new JsonResult(partner);
    }

    [HttpPut(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> Update([FromBody] UpdatePartnerRequest req)
    {
        var result = await _manager.UpdatePartnerAsync(User, req);
        if (result == null)
        {
            return NotFound(); // Partner not found or user doesn't have permission
        }
        return Ok(result);
    }

    [HttpDelete(APIDictionary.Partner + "/{id}")]
    [AccessControlled(EntityTypes.Partner, "delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _manager.DeletePartnerAsync(User, id);
        if (!success)
        {
            return NotFound(); // Partner not found or user doesn't have permission
        }
        return NoContent();
    }

    [HttpGet(APIDictionary.Partner + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var partner = await _manager.GetPartnerAsync(User, id);
        if (partner == null)
        {
            return NotFound();
        }

        // Return permissions for this partner
        var permissions = new { CanRead = true, CanUpdate = true, CanDelete = true };
        
        return Ok(permissions);
    }

    [HttpPost(APIDictionary.Partner + "/{id}/logo")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded");
        }

        // Check file size (1MB max)
        if (file.Length > 1024 * 1024)
        {
            return BadRequest("File size exceeds maximum limit of 1MB");
        }

        // Validate file type
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!validImageTypes.Contains(file.ContentType))
        {
            return BadRequest("Invalid file type. Only JPEG, PNG, and WEBP files are allowed.");
        }

        var result = await _manager.UpdatePartnerLogoAsync(id, file);
        return Ok(new { imageUrl = result });
    }

    [HttpGet(APIDictionary.Partner + "/by-partner-group-code/{code}")]
    // [AccessControlled(EntityTypes.Partner, "read", applyColumnFiltering: true, applyRowFiltering: true)]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerGroup(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _manager.GetPartnersByPartnerGroupAsync(User, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {       
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet(APIDictionary.Partner + "/by-partner-category-code/{code}")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerCategory(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _manager.GetPartnersByCategoryAsync(User, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
