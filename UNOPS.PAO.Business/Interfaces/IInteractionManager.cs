namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Models;

public interface IInteractionManager
{
    Task<InteractionModel> CreateInteractionAsync(InteractionRequest model);

    PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request);

    Task<InteractionModel?> GetInteraction(int userId, int id);

    Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model);

    Task DeleteInteractionAsync(int userId, int id);
} 