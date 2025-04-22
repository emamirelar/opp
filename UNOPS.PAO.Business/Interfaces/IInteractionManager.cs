namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;

public interface IInteractionManager
{
    Task<InteractionModel> CreateInteractionAsync(InteractionRequest model);

    PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request);

    PaginationResponse<InteractionModel> GetInteractionsWithSpecification(int userId, ISpecification<Domain.Entities.Interaction> specification, PaginationRequest pagination);

    Task<InteractionModel?> GetInteraction(int userId, int id);

    Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model);

    Task DeleteInteractionAsync(int userId, int id);

    PaginationResponse<InteractionModel> GetContactInteractionsAsync(int contactId, PaginationRequest request);
} 