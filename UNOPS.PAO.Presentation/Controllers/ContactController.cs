namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

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
    public ActionResult GetAll()
    {
        return Ok(manager.GetContacts(currentUserId));
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
}
