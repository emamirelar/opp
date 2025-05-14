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

[Route("/")]
[ApiController]
[Authorize]
public class ContactController : ControllerBase
{
    private IContactManager manager;
    private IAuthorizationService authorizationService;

    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public ContactController(IManagerWrapper manager, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
    {
        this.manager = manager.ContactManager;
        this.userResolverService = userResolverService;
        this.authorizationService = authorizationService;
    }

    [HttpPost(APIDictionary.Contact)]
    // Internal call: Create a Contact
    public async Task<IActionResult> Create([FromBody] ContactRequest req)
    {
        var result = await manager.CreateContactAsync(req);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.Contact)]
    // Internal call: get contacts created by logged-in user
    // TODO add permissions
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
                return BadRequest(new { error = ex.Message, searchCriteria });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to process advanced search criteria", details = ex.Message, searchCriteria });
            }
        }

        Debug.WriteLine($"Creating specification with AdvancedSearch={request.AdvancedSearch}, SearchCriteria={(request.SearchCriteria ?? "null")}");
        var specification = new ContactCompositeSpecification(request);
        return manager.GetContactsWithSpecification(currentUserId, specification, request);
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
        
        return Ok(manager.GetContactsWithSpecification(currentUserId, specification, request));
    }


    [HttpGet(APIDictionary.Contact + "/{id}")]
    // Internal call: Contact details
    // TODO add permissions

    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetContact(currentUserId, id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }

    [HttpPut(APIDictionary.Contact)]
    // Internal call: update Contact
    public async Task<IActionResult> Update([FromBody] UpdateContactRequest req)
    {
        await manager.UpdateContactAsync(currentUserId, req);

        return NoContent();
    }

    [HttpDelete(APIDictionary.Contact + "/{id}")]
    // Internal call: delete Contact
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteContactAsync(currentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.PartnerContacts)]
    // Internal call: List contacts for an specific partner
    public ActionResult PartnerContacts(int partnerId)
    {
        return Ok(manager.GetPartnerContacts(partnerId));
    }

    [HttpGet(APIDictionary.Contact + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var Contact = await manager.GetContact(currentUserId, id);

        if (Contact == null)
        {
            return NotFound();
        }

        var canReadResult = await authorizationService.AuthorizeAsync(User, Contact, Operations.Read);
        var canUpdateResult = await authorizationService.AuthorizeAsync(User, Contact, Operations.Update);
        var canCreateResult = await authorizationService.AuthorizeAsync(User, Contact, Operations.Create);
        var canDeleteResult = await authorizationService.AuthorizeAsync(User, Contact, Operations.Delete);

        return Ok(new
        {
            CanRead = canReadResult.Succeeded,
            CanUpdate = canUpdateResult.Succeeded,
            CanCreate = canCreateResult.Succeeded,
            CanDelete = canDeleteResult.Succeeded
        });
    }

    [HttpPost(APIDictionary.Contact + "/{id}/profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(int id, IFormFile file)
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

        var result = await manager.UpdateContactProfilePictureAsync(id, file);
        return Ok(new { imageUrl = result });
    }
}
