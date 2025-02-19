namespace UNOPS.PAO.Presentation.Controllers.External;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

[Route("/")]
[ApiController]
public class ContactController : ControllerBase
{
    private IContactManager manager;
    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public ContactController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager.ContactManager;
        this.userResolverService = userResolverService;
    }

    [HttpGet(APIDictionary.ExternalContact)]
    // External call: list of posted contacts
    public ActionResult GetPostedContacts()
    {
        return Ok(manager.GetPostedContacts());
    }

    [HttpGet(APIDictionary.ExternalContact + "/{id}")]
    // External call: details for posted contact
    public async Task<ActionResult> GetPostedFundingOportunity(int id)
    {
        var x = await manager.GetPostedContact(id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }
}
