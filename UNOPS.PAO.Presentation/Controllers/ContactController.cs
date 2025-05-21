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

[Route("/")]
public class ContactController : BaseController
{
    private readonly IContactManager _manager;

    public ContactController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<ContactController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.ContactManager;
    }

    [HttpPost(APIDictionary.Contact)]
    [AutoAuthorize]
    public async Task<ActionResult> Create([FromBody] ContactRequest req)
    {
        var result = await _manager.CreateContactAsync(req);
        if (result == null)
        {
            throw new BusinessException("Failed to create contact");
        }
        return StatusCode(201, result);
    }

    [HttpGet(APIDictionary.Contact)]
    [AutoAuthorize]
    public ActionResult<PaginationResponse<ContactModel>> GetAll([FromQuery] ContactFilterRequest request, [FromQuery] bool advancedSearch = false, [FromQuery] string searchCriteria = null)
    {
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
        return _manager.GetContactsWithSpecification(CurrentUserId, specification, request);
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


    [HttpGet(APIDictionary.Contact + "/{id}")]
    [AutoAuthorize]
    public async Task<ActionResult> Get(int id)
    {
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound();
        }
        
        // Return contact data directly using JsonResult
        return new JsonResult(contact);
    }

    [HttpPut(APIDictionary.Contact)]
    [AutoAuthorize]
    public async Task<ActionResult> Update([FromBody] UpdateContactRequest req)
    {
        await _manager.UpdateContactAsync(CurrentUserId, req);
        return NoContent();
    }

    [HttpDelete(APIDictionary.Contact + "/{id}")]
    [AutoAuthorize]
    public async Task<ActionResult> Delete(int id)
    {
        await _manager.DeleteContactAsync(CurrentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.PartnerContacts)]
    [AutoAuthorize]
    public ActionResult PartnerContacts(int partnerId)
    {
        return Ok(_manager.GetPartnerContacts(partnerId));
    }

    [HttpGet(APIDictionary.Contact + "/{id}/permissions")]
    [AutoAuthorize]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound(new { error = $"Contact with ID {id} not found" });
        }
        
        return Ok(await GetEntityPermissionsAsync(contact));
    }

    [HttpPost(APIDictionary.Contact + "/{id}/profile-picture")]
    [AutoAuthorize]
    public async Task<ActionResult> UploadProfilePicture(int id, IFormFile file)
    {
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
