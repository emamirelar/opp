namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

[Route("/")]
[ApiController]
[Authorize]
public class PartnerTreeController : ControllerBase
{
    private IPartnerTreeManager manager;
    private IAuthorizationService authorizationService;

    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public PartnerTreeController(IManagerWrapper manager, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
    {
        this.manager = manager.PartnerTreeManager;
        this.userResolverService = userResolverService;
        this.authorizationService = authorizationService;
    }

    [HttpPost(APIDictionary.PartnerTree)]
    // Internal call: Create a Partner Tree
    public async Task<IActionResult> Create([FromBody] PartnerTreeRequest req)
    {
        var result = await manager.CreatePartnerTreeAsync(req);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.PartnerTree)]
    // Internal call: get partner tree created by logged-in user
    // TODO add permissions
    public ActionResult GetAll()
    {
        return Ok(manager.GetPartnerTrees(currentUserId));
    }

    [HttpGet(APIDictionary.PartnerTree + "/{id}")]
    // Internal call: Partner Tree details
    // TODO add permissions

    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetPartnerTree(currentUserId, id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }

    [HttpPut(APIDictionary.PartnerTree)]
    // Internal call: update Partner Tree
    public async Task<IActionResult> Update([FromBody] UpdatePartnerTreeRequest req)
    {
        await manager.UpdatePartnerTreeAsync(currentUserId, req);

        return NoContent();
    }

    [HttpDelete(APIDictionary.PartnerTree + "/{id}")]
    // Internal call: delete partner tree
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeletePartnerTreeAsync(currentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.PartnerTree + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var PartnerTree = await manager.GetPartnerTree(currentUserId, id);

        if (PartnerTree == null)
        {
            return NotFound();
        }

        var canReadResult = await authorizationService.AuthorizeAsync(User, PartnerTree, Operations.Read);
        var canUpdateResult = await authorizationService.AuthorizeAsync(User, PartnerTree, Operations.Update);
        var canCreateResult = await authorizationService.AuthorizeAsync(User, PartnerTree, Operations.Create);
        var canDeleteResult = await authorizationService.AuthorizeAsync(User, PartnerTree, Operations.Delete);

        return Ok(new
        {
            CanRead = canReadResult.Succeeded,
            CanUpdate = canUpdateResult.Succeeded,
            CanCreate = canCreateResult.Succeeded,
            CanDelete = canDeleteResult.Succeeded
        });
    }
}
