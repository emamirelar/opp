namespace UNOPS.PAO.Presentation.Controllers.External;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("api/external/[controller]")]
[ApiController]
public class ProposalController : ControllerBase
{
    private IProposalManager manager;
    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public ProposalController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager.ProposalManager;
        this.userResolverService = userResolverService;
    }

    [HttpPost(APIDictionary.ExternalProposal)]
    public async Task<IActionResult> Create([FromBody] ProposalRequest req)
    {
        var result = await manager.CreateProposalAsync(currentUserId, req);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.ExternalProposal)]
    public ActionResult GetAll()
    {
        return Ok(manager.GetApplicantProposals(currentUserId));
    }

    [HttpGet(APIDictionary.ExternalProposal + "/{id}")]
    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetApplicantProposalByIdAsync(currentUserId, id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }

    [HttpPut(APIDictionary.ExternalProposal)]
    public async Task<IActionResult> Update([FromBody] UpdateProposalRequest req)
    {
        await manager.UpdateProposalAsync(currentUserId, req);

        return NoContent();
    }
}