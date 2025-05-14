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
    // Internal call: Create a Contact
    public async Task<ActionResult> Create([FromBody] ContactRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            var result = await _manager.CreateContactAsync(req);
            if (result == null)
            {
                throw new BusinessException("Failed to create contact");
            }
            return result;
        }, 201);
    }

    [HttpGet(APIDictionary.Contact)]
    // Internal call: get contacts created by logged-in user
    public ActionResult<PaginationResponse<ContactModel>> GetAll([FromQuery] ContactFilterRequest request, [FromQuery] bool advancedSearch = false, [FromQuery] string searchCriteria = null)
    {
        try
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
                    return BadRequest(new { error = ex.Message, searchCriteria });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "Failed to process advanced search criteria", details = ex.Message, searchCriteria });
                }
            }

            Debug.WriteLine($"Creating specification with AdvancedSearch={request.AdvancedSearch}, SearchCriteria={(request.SearchCriteria ?? "null")}");
            var specification = new ContactCompositeSpecification(request);
            return _manager.GetContactsWithSpecification(CurrentUserId, specification, request);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    [HttpGet(APIDictionary.Contact + "/{id}")]
    // Internal call: Contact details
    public async Task<ActionResult> Get(int id)
    {
        return await HandleOperationAsync(async () => 
        {
            var contact = await _manager.GetContact(CurrentUserId, id);
            if (contact == null)
            {
                throw new BusinessException($"Contact with ID {id} not found");
            }
            return contact;
        });
    }

    [HttpPut(APIDictionary.Contact)]
    // Internal call: update Contact
    public async Task<ActionResult> Update([FromBody] UpdateContactRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            await _manager.UpdateContactAsync(CurrentUserId, req);
        });
    }

    [HttpDelete(APIDictionary.Contact + "/{id}")]
    // Internal call: delete Contact
    public async Task<ActionResult> Delete(int id)
    {
        return await HandleOperationAsync(async () => 
        {
            await _manager.DeleteContactAsync(CurrentUserId, id);
        });
    }

    [HttpGet(APIDictionary.PartnerContacts)]
    // Internal call: List contacts for an specific partner
    public ActionResult PartnerContacts(int partnerId)
    {
        return Ok(_manager.GetPartnerContacts(partnerId));
    }

    [HttpGet(APIDictionary.Contact + "/{id}/permissions")]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        return await HandleOperationAsync(async () => 
        {
            var contact = await _manager.GetContact(CurrentUserId, id);
            if (contact == null)
            {
                throw new BusinessException($"Contact with ID {id} not found");
            }
            return await GetEntityPermissionsAsync(contact);
        });
    }

    [HttpPost(APIDictionary.Contact + "/{id}/profile-picture")]
    public async Task<ActionResult> UploadProfilePicture(int id, IFormFile file)
    {
        return await HandleOperationAsync(async () => 
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
            return new { imageUrl = result };
        });
    }
}
