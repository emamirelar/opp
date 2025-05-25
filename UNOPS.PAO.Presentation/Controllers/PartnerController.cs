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
using UNOPS.PAO.UNOPSBusiness.Authorization;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerController : BaseController
{
    private readonly IPartnerManager _manager;

    public PartnerController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerController> logger,
        IPermissionService permissionService)
        : base(logger, authorizationService, userResolverService, permissionService)
    {
        _manager = manager.PartnerManager;
    }

    [HttpPost(APIDictionary.Partner)]
    public async Task<IActionResult> Create([FromBody] PartnerRequest req)
    {
        // Check permission to create partners
        var permissionResult = await CheckEntityPermissionAsync("Partner", "create");
        if (permissionResult != null) return permissionResult;
        
        var result = await _manager.CreatePartnerAsync(User, req);
        if (result == null)
        {
            return Forbid(); // User doesn't have permission to create partners
        }
        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.Partner)]
    [AutoAuthorize]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetAll([FromQuery] PartnerFilterRequest request, [FromQuery] bool advancedSearch = false, [FromQuery] string searchCriteria = null)
    {
        if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
        {
            try
            {
                var newRequest = AdvancedSearchHelper.MapAdvancedSearchCriteria<PartnerFilterRequest>(searchCriteria);
                // Copy over any properties that weren't in the search criteria but were in the original request
                foreach (var prop in typeof(PartnerFilterRequest).GetProperties())
                {
                    if (prop.GetValue(newRequest) == null)
                    {
                        prop.SetValue(newRequest, prop.GetValue(request));
                    }
                }
                request = newRequest;
            }
            catch (ArgumentException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Failed to process advanced search criteria: {ex.Message}");
            }
        }

        var specification = new PartnerCompositeSpecification(request);
        return Ok(await _manager.GetPartnersWithSpecification(CurrentUserId, specification, request));
    }
    
    [HttpGet(APIDictionary.Partner + "/classic-search" )]
    // Internal call: get Partners created by logged-in user
    // TODO add permissions
    public async Task<ActionResult> GetAllClassicSearch([FromQuery] PartnerFilterRequest request)
    {
        var specification = new PartnerCompositeClassicSearchSpecification(
            id: request.Id,
            name: request.Name,
            status: request.Status,
            newEngagement: request.NewEngagement,
            phone: request.Phone,
            website: request.Website,
            shortName: request.ShortName,
            partnerOfficeId: request.PartnerOfficeId,
            partnerCategoryId: request.PartnerCategoryId,
            addressCity: request.AddressCity,
            addressStateProvince: request.AddressStateProvince,
            addressPostalCode: request.AddressPostalCode,
            addressCountry: request.AddressCountry,
            searchText: request.SearchText);

        return Ok(await _manager.GetPartnersWithSpecification(CurrentUserId, specification, request));
    }


    [HttpGet(APIDictionary.Partner + "/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // Check permission to read partners
        var permissionResult = await CheckEntityPermissionAsync("Partner", "read");
        if (permissionResult != null) return permissionResult;
        
        var partner = await _manager.GetPartnerAsync(User, id);
        if (partner == null)
        {
            return NotFound();
        }

        // Return partner data directly using JsonResult to avoid the wrapper metadata
        return new JsonResult(partner);
    }

    [HttpPut(APIDictionary.Partner)]
    public async Task<IActionResult> Update([FromBody] UpdatePartnerRequest req)
    {
        // Check permission to update partners
        var permissionResult = await CheckEntityPermissionAsync("Partner", "update");
        if (permissionResult != null) return permissionResult;
        
        var result = await _manager.UpdatePartnerAsync(User, req);
        if (result == null)
        {
            return NotFound(); // Partner not found or user doesn't have permission
        }
        return Ok(result);
    }

    [HttpDelete(APIDictionary.Partner + "/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Check permission to delete partners
        var permissionResult = await CheckEntityPermissionAsync("Partner", "delete");
        if (permissionResult != null) return permissionResult;
        
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
        var permissions = await GetEntityPermissionsAsync("Partner", partner);
        
        return Ok(permissions);
    }

    [HttpPost(APIDictionary.Partner + "/{id}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        // Get the partner to check permissions on it
        var partner = await _manager.GetPartnerAsync(User, id);
        if (partner == null)
        {
            return NotFound();
        }
        
        // Check update permission for this specific partner
        var permissionResult = await CheckEntityPermissionAsync("Partner", "update", partner);
        if (permissionResult != null) return permissionResult;
        
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
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerGroup(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _manager.GetPartnersByPartnerGroup(CurrentUserId, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {       
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet(APIDictionary.Partner + "/by-partner-category-code/{code}")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerCategory(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _manager.GetPartnersByPartnerCategory(CurrentUserId, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
