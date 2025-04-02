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
public class PartnerController : ControllerBase
{
    private IPartnerManager manager;
    private IAuthorizationService authorizationService;

    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public PartnerController(IManagerWrapper manager, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
    {
        this.manager = manager.PartnerManager;
        this.userResolverService = userResolverService;
        this.authorizationService = authorizationService;
    }

    [HttpPost(APIDictionary.Partner)]
    // Internal call: Create a Partner
    public async Task<IActionResult> Create([FromBody] PartnerRequest req)
    {
        var result = await manager.CreatePartnerAsync(req);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.Partner)]
    // Internal call: get Partners created by logged-in user
    // TODO add permissions
    public ActionResult GetAll([FromQuery] PaginationRequest parameters)
    {
        return Ok(manager.GetPartners(currentUserId, parameters));
    }

    [HttpGet(APIDictionary.Partner + "/{id}")]
    // Internal call: Partner details
    // TODO add permissions

    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetPartner(currentUserId, id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }

    [HttpPut(APIDictionary.Partner)]
    // Internal call: update Partner
    public async Task<IActionResult> Update([FromBody] UpdatePartnerRequest req)
    {
        await manager.UpdatePartnerAsync(currentUserId, req);

        return NoContent();
    }

    [HttpDelete(APIDictionary.Partner + "/{id}")]
    // Internal call: delete Partner
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeletePartnerAsync(currentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.Partner + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var Partner = await manager.GetPartner(currentUserId, id);

        if (Partner == null)
        {
            return NotFound();
        }

        var canReadResult = await authorizationService.AuthorizeAsync(User, Partner, Operations.Read);
        var canUpdateResult = await authorizationService.AuthorizeAsync(User, Partner, Operations.Update);
        var canCreateResult = await authorizationService.AuthorizeAsync(User, Partner, Operations.Create);
        var canDeleteResult = await authorizationService.AuthorizeAsync(User, Partner, Operations.Delete);

        return Ok(new
        {
            CanRead = canReadResult.Succeeded,
            CanUpdate = canUpdateResult.Succeeded,
            CanCreate = canCreateResult.Succeeded,
            CanDelete = canDeleteResult.Succeeded
        });
    }
}
