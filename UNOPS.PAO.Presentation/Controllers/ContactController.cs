using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;

namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Security;
using System.Text.Json.Nodes;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation;
using UNOPS.PAO.UNOPSBusiness.Authorization;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class ContactController : BaseController
{
    private readonly IContactManager _manager;

    public ContactController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        ILogger<ContactController> logger,
        IAuthorizationService authorizationService,
        IPermissionService permissionService)
        : base(logger, authorizationService, userResolverService, permissionService)
    {
        _manager = manager.ContactManager;
    }

    [HttpPost(APIDictionary.Contact)]
    public async Task<ActionResult> Create([FromBody] ContactRequest req)
    {
        // Check permission to create contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "create");
        if (permissionResult != null) return permissionResult;
        
        var result = await _manager.CreateContactAsync(req);
        if (result == null)
        {
            throw new BusinessException("Failed to create contact");
        }
        return StatusCode(201, result);
    }

    [HttpGet(APIDictionary.Contact)]
    public async Task<ActionResult> Get(
        [FromQuery] bool advancedSearch = false, 
        [FromQuery] string? searchCriteria = null, 
        [FromQuery] string? searchText = null,
        [FromQuery] int pageIndex = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? orderBy = null, 
        [FromQuery] bool? ascending = null)
    {
        // Check permission to read contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "read");
        if (permissionResult != null) return permissionResult;
        
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(pageIndex, pageSize);
        if (validationResult != null) return validationResult;
        
        // Create pagination request
        var paginationRequest = new PaginationRequest(pageIndex, pageSize, orderBy, ascending);
        
        // Handle different search scenarios using the new helper methods
        return await HandleSearchOperationAsync(async () =>
        {
            if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
            {
                return SearchControllerHelper.ProcessAdvancedSearchSync<ContactFilterRequest, ContactCompositeSpecification, object>(
                    searchCriteria, searchText, pageIndex, pageSize, orderBy, ascending, paginationRequest,
                    "Contact",
                    filterRequest => new ContactCompositeSpecification(filterRequest),
                    (userId, spec, pagination) => _manager.GetContactsWithSpecification(userId, spec, pagination),
                    CurrentUserId, _logger);
            }
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                return SearchControllerHelper.ProcessSimpleTextSearchSync<ContactFilterRequest, ContactCompositeSpecification, object>(
                    searchText, pageIndex, pageSize, orderBy, ascending, paginationRequest,
                    "Contact",
                    filterRequest => new ContactCompositeSpecification(filterRequest),
                    (userId, spec, pagination) => _manager.GetContactsWithSpecification(userId, spec, pagination),
                    CurrentUserId, _logger);
            }
            
            // Return all contacts with pagination
            _logger.LogInformation("Retrieving all contacts with pagination");
            return _manager.GetContacts(CurrentUserId, paginationRequest);
        }, "contact search");
    }

    [HttpGet(APIDictionary.Contact + "/{id}")]
    public async Task<ActionResult> Get(int id)
    {
        // Check permission to read contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "read");
        if (permissionResult != null) return permissionResult;
        
        // Use the new secure method that checks entity-level access
        var contact = await _manager.GetContactAsync(User, id);
        if (contact == null)
        {
            return NotFound();
        }
        return Ok(contact);
    }

    [HttpPut(APIDictionary.Contact)]
    public async Task<ActionResult> Update([FromBody] UpdateContactRequest req)
    {
        // Check permission to update contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "update");
        if (permissionResult != null) return permissionResult;
        
        // Use the new secure method that checks entity-level permissions
        var result = await _manager.UpdateContactAsync(User, req);
        return Ok(result);
    }

    [HttpDelete(APIDictionary.Contact + "/{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        // Check permission to delete contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "delete");
        if (permissionResult != null) return permissionResult;
        
        // Use the new secure method that checks entity-level permissions
        await _manager.DeleteContactAsync(User, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.PartnerContacts)]
    public async Task<ActionResult> PartnerContacts(int partnerId)
    {
        // Check permission to read contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "read");
        if (permissionResult != null) return permissionResult;
        
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
        var permissions = await GetEntityPermissionsAsync("Contact", contact);
        
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
        var permissionResult = await CheckEntityPermissionAsync("Contact", "update", contact);
        if (permissionResult != null) return permissionResult;
        
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
