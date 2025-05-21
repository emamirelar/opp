using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionManager _manager;

        public InteractionController(
            IManagerWrapper manager, 
            UserResolverService<int> userResolverService, 
            IAuthorizationService authorizationService,
            ILogger<InteractionController> logger)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.InteractionManager;
        }

        [HttpPost(APIDictionary.Interaction)]
        public async Task<ActionResult> Create([FromBody] InteractionRequest req)
        {
            return await HandleOperationAsync(async () =>
            {
                var result = await _manager.CreateInteractionAsync(req);
                if (result == null)
                {
                    throw new BusinessException("Failed to create interaction");
                }
                return result;
            }, 201);
        }

        [HttpGet(APIDictionary.Interaction)]
        public ActionResult GetAll([FromQuery] InteractionFilterRequest request)
        {
            try
            {
                var specification = new InteractionCompositeSpecification(
                    contactId: request.ContactId,
                    type: request.Type,
                    fromDate: request.FromDate,
                    toDate: request.ToDate,
                    searchText: request.SearchText);
                
                return Ok(_manager.GetInteractionsWithSpecification(CurrentUserId, specification, request));
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        [HttpGet(APIDictionary.Interaction + "/{id}")]
        public async Task<ActionResult> Get(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                var interaction = await _manager.GetInteraction(CurrentUserId, id);
                if (interaction == null)
                {
                    throw new BusinessException($"Interaction with ID {id} not found");
                }
                return interaction;
            });
        }

        [HttpPut(APIDictionary.Interaction)]
        public async Task<ActionResult> Update([FromBody] UpdateInteractionRequest req)
        {
            return await HandleOperationAsync(async () =>
            {
                await _manager.UpdateInteractionAsync(CurrentUserId, req);
            });
        }

        [HttpDelete(APIDictionary.Interaction + "/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                await _manager.DeleteInteractionAsync(CurrentUserId, id);
            });
        }

        [HttpGet(APIDictionary.Interaction + "/{id}/permissions")]
        public async Task<ActionResult> PermissionsGet(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                var interaction = await _manager.GetInteraction(CurrentUserId, id);
                if (interaction == null)
                {
                    throw new BusinessException($"Interaction with ID {id} not found");
                }
                return await GetEntityPermissionsAsync(interaction);
            });
        }
    }
} 