namespace UNOPS.PAO.Presentation.Controllers.External;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

[Route("/")]
[ApiController]
public class FundingOpportunityController : ControllerBase
{
    private IFundingOpportunityManager manager;
    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public FundingOpportunityController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager.FundingOpportunityManager;
        this.userResolverService = userResolverService;
    }

    [HttpGet(APIDictionary.ExternalFundingOpportunity)]
    // External call: list of posted funding opportunities
    public ActionResult GetPostedFundingOpportunities()
    {
        return Ok(manager.GetPostedFundingOpportunities());
    }

    [HttpGet(APIDictionary.ExternalFundingOpportunity + "/{id}")]
    // External call: details for posted opportunity
    public async Task<ActionResult> GetPostedFundingOportunity(int id)
    {
        var x = await manager.GetPostedFundingOpportunity(id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }
}
