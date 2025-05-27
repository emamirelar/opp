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
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionManager _manager;
        private readonly IBusinessSecurityService _businessSecurityService;

        public InteractionController(
            IManagerWrapper manager, 
            UserResolverService<int> userResolverService, 
            IAuthorizationService authorizationService,
            IBusinessSecurityService businessSecurityService,
            ILogger<InteractionController> logger)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.InteractionManager;
            _businessSecurityService = businessSecurityService;
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
        public async Task<ActionResult> GetAll([FromQuery] InteractionFilterRequest request)
        {
            return await HandleOperationAsync(async () =>
            {
                var specification = new InteractionCompositeSpecification(
                    contactId: request.ContactId,
                    type: request.Type,
                    fromDate: request.FromDate,
                    toDate: request.ToDate,
                    searchText: request.SearchText);
                
                var result = _manager.GetInteractionsWithSpecification(CurrentUserId, specification, request);
                
                // Apply RBAC filtering to the results
                if (result.Records?.Any() == true)
                {
                    var filteredData = new List<InteractionModel>();
                    foreach (var interaction in result.Records)
                    {
                        // Create a minimal interaction entity for permission checking
                        var interactionEntity = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSInteraction
                        {
                            Id = interaction.Id,
                            ContactId = interaction.ContactId,
                            Contact = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSContact { Id = interaction.ContactId }
                        };
                        
                        if (await _businessSecurityService.CanUserAccessEntityAsync(interactionEntity, User, "read"))
                        {
                            filteredData.Add(interaction);
                        }
                    }
                    
                    result.Records = filteredData;
                    result.TotalCount = filteredData.Count;
                }
                
                return result;
            });
        }

        [HttpGet(APIDictionary.Interaction + "/{id}")]
        public async Task<ActionResult> Get(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                // Use the secure method that includes permissions in the interaction model
                var interaction = await _manager.GetInteractionAsync(User, id);
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

                // Create entity for permission checking
                var interactionEntity = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSInteraction
                {
                    Id = interaction.Id,
                    ContactId = interaction.ContactId,
                    Contact = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSContact { Id = interaction.ContactId }
                };

                return await _businessSecurityService.GetEntityPermissionsAsync(interactionEntity, User);
            });
        }
    }
} 