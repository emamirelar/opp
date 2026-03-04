namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Specifications;
using System.Security.Claims;
using UNOPS.PAO.Models.Integrations;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;

public interface IInteractionManager
{
    Task<InteractionModel> CreateInteractionAsync(InteractionRequest model);

    PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request);

    Task<PaginationResponse<InteractionModel>> GetInteractionsWithSpecification(int userId, ISpecification<Domain.Entities.Interaction> specification, PaginationRequest pagination);

    Task<InteractionModel?> GetInteraction(int userId, int id);

    IEnumerable<ExternalInteractionModel> GetPostedInteractions();

    Task<ExternalInteractionModel?> GetPostedInteraction(int id);

    Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model);

    Task DeleteInteractionAsync(int userId, int id);

    Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request);

    Task<PaginationResponse<InteractionModel>> GetContactInteractionsAsync(int contactId, PaginationRequest request);

    Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request);
    
    Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id);
    
    Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model);
    
    Task DeleteInteractionAsync(ClaimsPrincipal user, int id);

    Task<InteractionModel?> FindGmailInteractionAsync(GmailInteractionRequest model);

    Task<InteractionModel?> CreateGmailInteractionAsync(InteractionRequest model);
    
    /// <summary>
    /// Get supported search fields for interactions
    /// </summary>
    List<SearchFieldInfo> GetInteractionSearchFields();
} 