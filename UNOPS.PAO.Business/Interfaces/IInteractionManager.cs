using UNOPS.PAO.RBAC.Attributes;

namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using System.Security.Claims;

public interface IInteractionManager
{
    Task<InteractionModel> CreateInteractionAsync(InteractionRequest model);

    PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request);

    PaginationResponse<InteractionModel> GetInteractionsWithSpecification(int userId, ISpecification<Domain.Entities.Interaction> specification, PaginationRequest pagination);

    Task<InteractionModel?> GetInteraction(int userId, int id);

    IEnumerable<ExternalInteractionModel> GetPostedInteractions();

    Task<ExternalInteractionModel?> GetPostedInteraction(int id);

    Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model);

    Task DeleteInteractionAsync(int userId, int id);

    Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request);

    PaginationResponse<InteractionModel> GetContactInteractionsAsync(int contactId, PaginationRequest request);

    // New secure methods with ClaimsPrincipal for row-level security
    
    /// <summary>
    /// Gets interactions with row-level filtering and entity permissions applied
    /// </summary>
    [RBAC("read", Entity = "Interaction", ApplyRowFiltering = true, ApplyColumnFiltering = true)]
    Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request);
    
    /// <summary>
    /// Gets a specific interaction with entity-level access check and permissions
    /// </summary>
    [RBAC("read", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "id", ApplyColumnFiltering = true)]
    Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id);
    
    /// <summary>
    /// Updates an interaction with entity-level access check
    /// </summary>
    [RBAC("update", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "model.Id")]
    Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model);
    
    /// <summary>
    /// Deletes an interaction with entity-level access check
    /// </summary>
    [RBAC("delete", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task DeleteInteractionAsync(ClaimsPrincipal user, int id);
} 