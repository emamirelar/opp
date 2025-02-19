namespace UNOPS.PAO.Presentation.Controllers;

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
public class FundingOpportunityController : ControllerBase
{
    private IFundingOpportunityManager manager;
    private IAuthorizationService authorizationService;

    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public FundingOpportunityController(IManagerWrapper manager, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
    {
        this.manager = manager.FundingOpportunityManager;
        this.userResolverService = userResolverService;
        this.authorizationService = authorizationService;
    }

    [HttpPost(APIDictionary.FundingOpportunity)]
    // Internal call: Create a funding opportunity
    public async Task<IActionResult> Create([FromBody] FundingOpportunityRequest req)
    {
        var result = await manager.CreateFundingOpportunityAsync(req);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.FundingOpportunity)]
    // Internal call: get funding opportunities created by logged-in user
    // TODO add permissions
    public ActionResult GetAll()
    {
        return Ok(manager.GetFundingOpportunities(currentUserId));
    }

    [HttpGet(APIDictionary.FundingOpportunity + "/{id}")]
    // Internal call: Funding opportunity details
    // TODO add permissions

    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetFundingOpportunity(currentUserId, id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }

    [HttpPut(APIDictionary.FundingOpportunity)]
    // Internal call: update funding opportunity
    public async Task<IActionResult> Update([FromBody] UpdateFundingOpportunityRequest req)
    {
        await manager.UpdateFundingOpportunityAsync(currentUserId, req);

        return NoContent();
    }

    [HttpDelete(APIDictionary.FundingOpportunity + "/{id}")]
    // Internal call: delete funding opportunity
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteFundingOpportunityAsync(currentUserId, id);
        return NoContent();
    }

    [HttpGet(APIDictionary.FundingOpportunity + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var fundingOpportunity = await manager.GetFundingOpportunity(currentUserId, id);

        if (fundingOpportunity == null)
        {
            return NotFound();
        }

        var canReadResult = await authorizationService.AuthorizeAsync(User, fundingOpportunity, Operations.Read);
        var canUpdateResult = await authorizationService.AuthorizeAsync(User, fundingOpportunity, Operations.Update);
        var canCreateResult = await authorizationService.AuthorizeAsync(User, fundingOpportunity, Operations.Create);
        var canDeleteResult = await authorizationService.AuthorizeAsync(User, fundingOpportunity, Operations.Delete);

        return Ok(new
        {
            CanRead = canReadResult.Succeeded,
            CanUpdate = canUpdateResult.Succeeded,
            CanCreate = canCreateResult.Succeeded,
            CanDelete = canDeleteResult.Succeeded
        });
    }
}
