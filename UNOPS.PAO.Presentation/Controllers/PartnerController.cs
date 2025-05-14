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
    public async Task<IActionResult> Create([FromBody] PartnerRequest req)
    {
        return await HandleOperationAsync<IActionResult>(async () =>
        {
            var result = await _manager.CreatePartnerAsync(req);
            if (result == null)
            {
                return BadRequest();
            }
            return CreatedAtAction(nameof(Create), result.Id, result);
        });
    }

    [HttpGet(APIDictionary.Partner)]
    public ActionResult<PaginationResponse<PartnerModel>> GetAll([FromQuery] PartnerFilterRequest request, [FromQuery] bool advancedSearch = false, [FromQuery] string searchCriteria = null)
    {
        try
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
                    _logger.LogWarning(ex, "Invalid search criteria: {SearchCriteria}", searchCriteria);
                    return BadRequest(new { error = ex.Message, searchCriteria });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process advanced search criteria: {SearchCriteria}", searchCriteria);
                    return BadRequest(new { error = "Failed to process advanced search criteria", details = ex.Message, searchCriteria });
                }
            }

            var specification = new PartnerCompositeSpecification(request);
            return _manager.GetPartnersWithSpecification(CurrentUserId, specification, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAll partners");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    [HttpGet(APIDictionary.Partner + "/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return await HandleOperationAsync<IActionResult>(async () =>
        {
            var partner = await _manager.GetPartner(CurrentUserId, id);
            if (partner == null)
            {
                return NotFound();
            }

            // Check permission
            var permissionResult = await CheckPermissionAsync(partner, Operations.Read);
            if (permissionResult != null)
            {
                return permissionResult;
            }

            return Ok(partner);
        });
    }

    [HttpPut(APIDictionary.Partner)]
    public async Task<IActionResult> Update([FromBody] UpdatePartnerRequest req)
    {
        return await HandleOperationAsync<IActionResult>(async () =>
        {
            // Get the partner to verify permissions
            var partner = await _manager.GetPartner(CurrentUserId, req.Id);
            if (partner == null)
            {
                return NotFound();
            }

            // Check permission
            var permissionResult = await CheckPermissionAsync(partner, Operations.Update);
            if (permissionResult != null)
            {
                return permissionResult;
            }

            await _manager.UpdatePartnerAsync(CurrentUserId, req);
            return NoContent();
        });
    }

    [HttpDelete(APIDictionary.Partner + "/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await HandleOperationAsync<IActionResult>(async () =>
        {
            // Get the partner to verify permissions
            var partner = await _manager.GetPartner(CurrentUserId, id);
            if (partner == null)
            {
                return NotFound();
            }

            // Check permission
            var permissionResult = await CheckPermissionAsync(partner, Operations.Delete);
            if (permissionResult != null)
            {
                return permissionResult;
            }

            await _manager.DeletePartnerAsync(CurrentUserId, id);
            return NoContent();
        });
    }

    [HttpGet(APIDictionary.Partner + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        return await HandleOperationAsync<IActionResult>(async () =>
        {
            var partner = await _manager.GetPartner(CurrentUserId, id);
            if (partner == null)
            {
                return NotFound();
            }

            return Ok(await GetEntityPermissionsAsync(partner));
        });
    }

    [HttpPost(APIDictionary.Partner + "/{id}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        return await HandleOperationAsync<IActionResult>(async () =>
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

            // Get the partner to verify permissions
            var partner = await _manager.GetPartner(CurrentUserId, id);
            if (partner == null)
            {
                return NotFound();
            }

            // Check permission
            var permissionResult = await CheckPermissionAsync(partner, Operations.Update);
            if (permissionResult != null)
            {
                return permissionResult;
            }

            var result = await _manager.UpdatePartnerLogoAsync(id, file);
            return Ok(new { imageUrl = result });
        });
    }
}
