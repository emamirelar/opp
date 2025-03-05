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
public class PartnerTreeController : ControllerBase
{
    private IPartnerTreeManager manager;
    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public PartnerTreeController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager.PartnerTreeManager;
        this.userResolverService = userResolverService;
    }

    [HttpGet(APIDictionary.ExternalPartnerTree)]
    // External call: list of posted partner tree
    public ActionResult GetPostedPartnerTrees()
    {
        return Ok(manager.GetPostedPartnerTrees());
    }

    [HttpGet(APIDictionary.ExternalPartnerTree + "/{id}")]
    // External call: details for posted partner tree
    public async Task<ActionResult> GetPostedPartnerTree(int id)
    {
        var x = await manager.GetPostedPartnerTree(id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }
}
