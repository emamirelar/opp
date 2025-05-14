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
using System.Collections.Generic;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerTreeController : ControllerBase
{
    private readonly IPartnerTreeManager manager;
    private readonly IAuthorizationService authorizationService;
    private readonly UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public PartnerTreeController(IManagerWrapper managerWrapper, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
    {
        this.manager = managerWrapper.PartnerTreeManager;
        this.userResolverService = userResolverService;
        this.authorizationService = authorizationService;
    }

    [HttpPost(APIDictionary.PartnerTree)]
    // Internal call: Create a Partner Tree
    public async Task<IActionResult> Create([FromBody] PartnerTreeDataModel req)
    { 
        var result = await manager.CreatePartnerTreeAsync(req);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), new { id = result.Data.Id }, result);
    }

    [HttpGet(APIDictionary.PartnerTree)]
    // Internal call: get partner tree created by logged-in user
    // TODO add permissions
    public ActionResult GetAll([FromQuery] string sortBy = "Name", [FromQuery] bool ascending = true)
    {
        return Ok(manager.GetPartnerTrees(currentUserId, sortBy, ascending));
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
    public async Task<IActionResult> Update([FromBody] PartnerTreeDataModel[] req)
    {
        List<PartnerTreeModel> updatedTrees = new List<PartnerTreeModel>();
        
        foreach (var item in req)
        {
            var updatedTree = await manager.UpdatePartnerTreeAsync(currentUserId, item);
            if (updatedTree != null)
            {
                updatedTrees.Add(updatedTree);
            }
        }

        // Return the updated trees with proper editability flags
        return Ok(updatedTrees);
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
