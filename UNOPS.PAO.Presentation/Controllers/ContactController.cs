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
    public async Task<ActionResult> Get([FromQuery] PaginationRequest request)
    {
        // Check permission to read contacts
        var permissionResult = await CheckEntityPermissionAsync("Contact", "read");
        if (permissionResult != null) return permissionResult;
        
        // Use the new secure method that includes row filtering and permissions
        var result = await _manager.GetContactsAsync(User, request);
        return Ok(result);
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

    [HttpGet(APIDictionary.Contact + "/classic-search")]
    public ActionResult GetAll([FromQuery] ContactFilterRequest request)
    {
        var specification = new ClassicContactCompositeSpecification(
            id: request.Id,
            partnerId: request.PartnerId,
            status: request.Status,
            salutation: request.Salutation,
            title: request.Title,
            department: request.Department,
            phone: request.Phone,
            mobile: request.Mobile,
            assistant: request.Assistant,
            assistantEmail: request.AssistantEmail,
            assistantPhone: request.AssistantPhone,
            mailingCity: request.MailingCity,
            mailingStateProvince: request.MailingStateProvince,
            mailingPostalCode: request.MailingPostalCode,
            mailingCountry: request.MailingCountry,
            searchText: request.SearchText);
        
        return Ok(_manager.GetContactsWithSpecification(CurrentUserId, specification, request));
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
