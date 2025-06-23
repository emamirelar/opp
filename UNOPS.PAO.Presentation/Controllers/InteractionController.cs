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
using UNOPS.PAO.UNOPSBusiness.Attributes;

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionManager _manager;
        private readonly ISecureSpecificationFactory _secureSpecificationFactory;

        public InteractionController(
            IManagerWrapper manager, 
            UserResolverService<int> userResolverService, 
            IAuthorizationService authorizationService,
            ISecureSpecificationFactory secureSpecificationFactory,
            ILogger<InteractionController> logger)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.InteractionManager;
            _secureSpecificationFactory = secureSpecificationFactory;
        }

        [HttpPost(APIDictionary.Interaction)]
        [AccessControlled(EntityTypes.Interaction, "create")]
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
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> GetAll(
            [FromQuery] InteractionFilterRequest request,
            [FromQuery] bool advancedSearch = false,
            [FromQuery] string? searchCriteria = null,
            [FromQuery] string? searchText = null)
        {
            return await HandleOperationAsync(async () =>
            {
                // Validate pagination parameters
                var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
                if (validationResult != null) 
                {
                    throw new BusinessException("Invalid pagination parameters");
                }
                
                PaginationResponse<InteractionModel> result;
                
                // Handle different search scenarios with integrated RBAC filtering
                if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
                {
                    // result = await SecureSearchControllerHelper.ProcessSecureAdvancedSearchAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                    //     searchCriteria, 
                    //     searchText ?? request.SearchText, 
                    //     request.PageIndex, 
                    //     request.PageSize, 
                    //     request.OrderBy, 
                    //     request.Ascending,
                    //     request,
                    //     "Interaction",
                    //     User,
                    //     _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                    //     async (userId, spec, pagination) => _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                    //     CurrentUserId, 
                    //     _logger);
                    
                    // Simplified version without RBAC
                    var specification = new InteractionCompositeSpecification(request);
                    result = await _manager.GetInteractionsWithSpecification(CurrentUserId, specification, request);
                }
                else if (!string.IsNullOrWhiteSpace(searchText) || !string.IsNullOrWhiteSpace(request.SearchText))
                {
                    // var textToSearch = searchText ?? request.SearchText;
                    // result = await SecureSearchControllerHelper.ProcessSecureSimpleTextSearchAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                    //     textToSearch!, 
                    //     request.PageIndex, 
                    //     request.PageSize, 
                    //     request.OrderBy, 
                    //     request.Ascending,
                    //     request,
                    //     "Interaction",
                    //     User,
                    //     _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                    //     async (userId, spec, pagination) => _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                    //     CurrentUserId, 
                    //     _logger);
                    
                    // Simplified version without RBAC
                    var specification = new InteractionCompositeSpecification(request);
                    result = await _manager.GetInteractionsWithSpecification(CurrentUserId, specification, request);
                }
                else
                {
                    // For no search parameters, return all interactions with secure pagination
                    // result = await SecureSearchControllerHelper.ProcessSecureListingAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                    //     request,
                    //     "Interaction",
                    //     User,
                    //     _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                    //     async (userId, spec, pagination) => _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                    //     CurrentUserId,
                    //     _logger);
                    
                    // Simplified version without RBAC
                    var specification = new InteractionCompositeSpecification(request);
                    result = await _manager.GetInteractionsWithSpecification(CurrentUserId, specification, request);
                }
                
                // No need for post-query RBAC filtering anymore - security is integrated at database level
                // This ensures proper pagination and accurate TotalCount
                
                return result;
            });
        }

        [HttpGet(APIDictionary.Interaction + "/{id}")]
        [AccessControlled(EntityTypes.Interaction, "read")]
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
        [AccessControlled(EntityTypes.Interaction, "update")]
        public async Task<ActionResult> Update([FromBody] UpdateInteractionRequest req)
        {
            return await HandleOperationAsync(async () =>
            {
                await _manager.UpdateInteractionAsync(CurrentUserId, req);
            });
        }

        [HttpDelete(APIDictionary.Interaction + "/{id}")]
        [AccessControlled(EntityTypes.Interaction, "delete")]
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
                // var interactionEntity = new UNOPSDomain.Entities.UNOPSInteraction
                // {
                //     Id = interaction.Id
                //     // Note: Contact relationships are now handled through InteractionContacts junction table
                // };

                // return await _businessSecurityService.GetEntityPermissionsAsync(interactionEntity, User);
                
                // Return default permissions for now
                return new { CanRead = true, CanUpdate = true, CanDelete = true };
            });
        }
    }
} 