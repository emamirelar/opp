using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using System.Text.Json.Nodes;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation;
using UNOPS.PAO.UNOPSBusiness.Authorization;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class ContactController : BaseFilteredController
{
    private readonly IContactManager _manager;
    private readonly ILogger<ContactController> _logger;
    private readonly UserResolverService<int> _userResolverService;

    // Get current user ID from resolver service
    private int CurrentUserId => _userResolverService.GetCurrentUserId();

    public ContactController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        ILogger<ContactController> logger,
        IPermissionService permissionService)
        : base(permissionService)
    {
        _manager = manager.ContactManager;
        _logger = logger;
        _userResolverService = userResolverService;
    }

    [HttpPost(APIDictionary.Contact)]
    public async Task<ActionResult> Create([FromBody] ContactRequest req)
    {
        // Check permission to create contacts
        if (!await _permissionService.CanPerformActionAsync("Contact", "create", User))
        {
            return Forbid();
        }
        
        var result = await _manager.CreateContactAsync(req);
        if (result == null)
        {
            throw new BusinessException("Failed to create contact");
        }
        return StatusCode(201, result);
    }

    [HttpGet(APIDictionary.Contact)]
    public async Task<ActionResult<PaginationResponse<ContactModel>>> GetAll([FromQuery] ContactFilterRequest request, [FromQuery] bool advancedSearch = false, [FromQuery] string searchCriteria = null)
    {
        _logger.LogInformation("Getting all contacts");
        // Check permission to read contacts
        if (!await _permissionService.CanPerformActionAsync("Contact", "read", User))
        {
            return Forbid();
        }
        
        if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
        {
            Debug.WriteLine($"SEARCH CRITERIA RAW: {searchCriteria}");
            
            // Log all the parameters that came in 
            Debug.WriteLine("Query Parameters:");
            foreach (var param in HttpContext.Request.Query)
            {
                Debug.WriteLine($"  {param.Key}: {param.Value}");
            }
            
            try
            {
                Debug.WriteLine($"Advanced search requested with criteria: {searchCriteria}");
                var newRequest = AdvancedSearchHelper.MapAdvancedSearchCriteria<ContactFilterRequest>(searchCriteria);
                
                // Copy over any properties that weren't in the search criteria but were in the original request
                foreach (var prop in typeof(ContactFilterRequest).GetProperties())
                {
                    if (prop.GetValue(newRequest) == null)
                    {
                        prop.SetValue(newRequest, prop.GetValue(request));
                    }
                }
                
                // Debug the processed request
                Debug.WriteLine($"Processed advanced search request:");
                foreach (var prop in typeof(ContactFilterRequest).GetProperties())
                {
                    Debug.WriteLine($"  {prop.Name}: {prop.GetValue(newRequest)}");
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

        Debug.WriteLine($"Creating specification with AdvancedSearch={request.AdvancedSearch}, SearchCriteria={(request.SearchCriteria ?? "null")}");
        var specification = new ContactCompositeSpecification(request);
        
        // Use the manager to get contacts but apply security filtering
        var result = _manager.GetContactsWithSpecification(CurrentUserId, specification, request);
        
        // Row-level security will be automatically applied by the database query permissions
        // when data is retrieved
        
        return result;
    }

    [HttpGet(APIDictionary.Contact + "/{id}")]
    public async Task<ActionResult> Get(int id)
    {
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound();
        }
        
        // Check permission for this specific contact entity
        if (!await _permissionService.CanPerformActionAsync("Contact", "read", User, contact))
        {
            return Forbid();
        }
        
        // Return contact data directly using JsonResult
        return new JsonResult(contact);
    }

    [HttpPut(APIDictionary.Contact)]
    public async Task<ActionResult> Update([FromBody] UpdateContactRequest req)
    {
        // Get the contact to check permissions on it
        var contact = await _manager.GetContact(CurrentUserId, req.Id);
        if (contact == null)
        {
            return NotFound();
        }
        
        // Check permission for this specific contact
        if (!await _permissionService.CanPerformActionAsync("Contact", "update", User, contact))
        {
            return Forbid();
        }
        
        await _manager.UpdateContactAsync(CurrentUserId, req);
        return NoContent();
    }

    [HttpDelete(APIDictionary.Contact + "/{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        // Get the contact to check permissions on it
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound();
        }
        
        // Check permission for this specific contact
        if (!await _permissionService.CanPerformActionAsync("Contact", "delete", User, contact))
        {
            return Forbid();
        }
        
        await _manager.DeleteContactAsync(CurrentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.PartnerContacts)]
    public async Task<ActionResult> PartnerContacts(int partnerId)
    {
        // Check permission to read contacts
        if (!await _permissionService.CanPerformActionAsync("Contact", "read", User))
        {
            return Forbid();
        }
        
        return Ok(_manager.GetPartnerContacts(partnerId));
    }

    [HttpGet(APIDictionary.Contact + "/{id}/permissions")]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound(new { error = $"Contact with ID {id} not found" });
        }
        
        // Return permissions for this contact
        var permissions = new
        {
            CanRead = await _permissionService.CanPerformActionAsync("Contact", "read", User, contact),
            CanCreate = await _permissionService.CanPerformActionAsync("Contact", "create", User, contact),
            CanUpdate = await _permissionService.CanPerformActionAsync("Contact", "update", User, contact),
            CanDelete = await _permissionService.CanPerformActionAsync("Contact", "delete", User, contact)
        };
        
        return Ok(permissions);
    }

    [HttpPost(APIDictionary.Contact + "/{id}/profile-picture")]
    public async Task<ActionResult> UploadProfilePicture(int id, IFormFile file)
    {
        // Get the contact to check permissions on it
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound();
        }
        
        // Check update permission for this specific contact
        if (!await _permissionService.CanPerformActionAsync("Contact", "update", User, contact))
        {
            return Forbid();
        }
        
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("No file was uploaded");
        }

        // Check file size (1MB max)
        if (file.Length > 1024 * 1024)
        {
            throw new BusinessException("File size exceeds maximum limit of 1MB");
        }

        // Validate file type
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!validImageTypes.Contains(file.ContentType))
        {
            throw new BusinessException("Invalid file type. Only JPEG, PNG, and WEBP files are allowed.");
        }

        var result = await _manager.UpdateContactProfilePictureAsync(id, file);
        return Ok(new { imageUrl = result });
    }
}
