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

[Route("/")]
public class PartnerController : BaseController
{
    private readonly IPartnerManager _manager;

    public PartnerController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.PartnerManager;
    }

    [HttpPost(APIDictionary.Partner)]
    [AutoAuthorize]
    public async Task<IActionResult> Create([FromBody] PartnerRequest req)
    {
        var result = await _manager.CreatePartnerAsync(req);
        if (result == null)
        {
            return BadRequest();
        }
        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.Partner)]
    [AutoAuthorize]
    public ActionResult<PaginationResponse<PartnerModel>> GetAll([FromQuery] PartnerFilterRequest request, [FromQuery] bool advancedSearch = false, [FromQuery] string searchCriteria = null)
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
        return _manager.GetPartnersWithSpecification(CurrentUserId, specification, request);
    }
    
    [HttpGet(APIDictionary.Partner + "/classic-search" )]
    // Internal call: get Partners created by logged-in user
    // TODO add permissions
    public ActionResult GetAllClassicSearch([FromQuery] PartnerFilterRequest request)
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

        return Ok(_manager.GetPartnersWithSpecification(CurrentUserId, specification, request));
    }


    [HttpGet(APIDictionary.Partner + "/{id}")]
    [AutoAuthorize]
    public async Task<IActionResult> Get(int id)
    {
        var partner = await _manager.GetPartner(CurrentUserId, id);
        if (partner == null)
        {
            return NotFound();
        }

        // Return partner data directly using JsonResult to avoid the wrapper metadata
        return new JsonResult(partner);
    }

    [HttpPut(APIDictionary.Partner)]
    [AutoAuthorize]
    public async Task<IActionResult> Update([FromBody] UpdatePartnerRequest req)
    {
        await _manager.UpdatePartnerAsync(CurrentUserId, req);
        return NoContent();
    }

    [HttpDelete(APIDictionary.Partner + "/{id}")]
    [AutoAuthorize]
    public async Task<IActionResult> Delete(int id)
    {
        await _manager.DeletePartnerAsync(CurrentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.Partner + "/{id}/permissions")]
    [AutoAuthorize]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var partner = await _manager.GetPartner(CurrentUserId, id);
        if (partner == null)
        {
            return NotFound();
        }

        return Ok(await GetEntityPermissionsAsync(partner));
    }

    [HttpPost(APIDictionary.Partner + "/{id}/logo")]
    [AutoAuthorize]
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
    public ActionResult<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroup(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = _manager.GetPartnersByPartnerGroup(CurrentUserId, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {       
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet(APIDictionary.Partner + "/by-partner-category-code/{code}")]
    public ActionResult<PaginationResponse<PartnerModel>> GetPartnersByPartnerCategory(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = _manager.GetPartnersByPartnerCategory(CurrentUserId, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
