using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Server.Controllers
{
    [Route("/")]
    [ApiController]
    public class ProposalController : ControllerBase
    {
        private IProposalManager manager;
        private UserResolverService<int> userResolverService;

        public ProposalController(IManagerWrapper manager, UserResolverService<int> userResolverService)
        {
            this.manager = manager.ProposalManager;
            this.userResolverService = userResolverService;
        }

        [HttpGet(APIDictionary.Proposal)]
        // Internal call: List proposals
        public ActionResult GetProposals()
        {
            return Ok(manager.GetProposals());
        }


        [HttpGet(APIDictionary.FundingOpportunityProposal)]
        // Internal call: List proposals for an specific opportunity
        public ActionResult GetFundingOpportunityProposals(int opportunityId)
        {
            return Ok(manager.GetFundingOpportunityProposals(opportunityId));
        }

        [HttpGet(APIDictionary.Proposal + "/{id}")]
        // Internal call: Proposal details
        public async Task<ActionResult> Get(int id)
        {
            var x = await manager.GetFundingOpportunityProposalByIdAsync(id);

            if (x == null)
            {
                return NotFound();
            }

            return Ok(x);
        }

    }
}
