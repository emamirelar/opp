using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

namespace UNOPS.PAO.Presentation.Controllers
{

    [Route("/")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class InteractionController : ControllerBase
    {
        private IInteractionManager manager;
        private IAuthorizationService authorizationService;

        private UserResolverService<int> userResolverService;

        private int currentUserId => userResolverService.GetCurrentUserId();

        public InteractionController(IManagerWrapper manager, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
        {
            this.manager = manager.InteractionManager;
            this.userResolverService = userResolverService;
            this.authorizationService = authorizationService;
        }

        [HttpPost(APIDictionary.Interaction)]
        public async Task<IActionResult> Create([FromBody] InteractionRequest req)
        {
            var result = await manager.CreateInteractionAsync(req);

            if (result == null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(Create), result.Id, result);
        }

        [HttpGet(APIDictionary.Interaction)]
        // TODO add permissions
        public ActionResult GetAll([FromQuery] InteractionFilterRequest request)
        {
            var specification = new InteractionCompositeSpecification(
                contactId: request.ContactId,
                type: request.Type,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                searchText: request.SearchText);
            
            return Ok(manager.GetInteractionsWithSpecification(currentUserId, specification, request));
        }

        [HttpGet(APIDictionary.Interaction + "/{id}")]
        // TODO add permissions
        public async Task<ActionResult> Get(int id)
        {
            var x = await manager.GetInteraction(currentUserId, id);

            if (x == null)
            {
                return NotFound();
            }

            return Ok(x);
        }

        [HttpPut(APIDictionary.Interaction)]
        public async Task<IActionResult> Update([FromBody] UpdateInteractionRequest req)
        {
            await manager.UpdateInteractionAsync(currentUserId, req);

            return NoContent();
        }

        [HttpDelete(APIDictionary.Interaction + "/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await manager.DeleteInteractionAsync(currentUserId, id);
            return NoContent();
        }

        [HttpGet(APIDictionary.Interaction + "/{id}/permissions")]
        public async Task<IActionResult> PermissionsGet(int id)
        {
            var interaction = await manager.GetInteraction(currentUserId, id);

            if (interaction == null)
            {
                return NotFound();
            }

            var canReadResult = await authorizationService.AuthorizeAsync(User, interaction, Operations.Read);
            var canUpdateResult = await authorizationService.AuthorizeAsync(User, interaction, Operations.Update);
            var canCreateResult = await authorizationService.AuthorizeAsync(User, interaction, Operations.Create);
            var canDeleteResult = await authorizationService.AuthorizeAsync(User, interaction, Operations.Delete);

            return Ok(new
            {
                CanRead = canReadResult.Succeeded,
                CanUpdate = canUpdateResult.Succeeded,
                CanCreate = canCreateResult.Succeeded,
                CanDelete = canDeleteResult.Succeeded
            });
        }
    }
} 