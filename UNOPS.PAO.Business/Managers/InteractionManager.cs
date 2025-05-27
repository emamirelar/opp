namespace UNOPS.PAO.Business.Managers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Business.Repositories;
using System.Security.Claims;
using System.Security.Principal;

public class InteractionManager : IInteractionManager
{
    private readonly IMapper mapper;
    private readonly DataRepository<Interaction> interactionRepository;
    //private readonly DataRepository<InteractionContact> interactionContactRepository;
    //private readonly DataRepository<InteractionPartner> interactionPartnerRepository;
    //private readonly DataRepository<InteractionUser> interactionUserRepository;
    private readonly AppDbContext context;

    public InteractionManager(IMapper mapper, AppDbContext context)
    {
        mapper = mapper;
        context = context;
        interactionRepository = new DataRepository<Interaction>(context);
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    private async Task ProcessJunctionTables(Interaction interaction, InteractionRequest model)
    {

        await using var jtTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Process InteractionContacts
            if (model.ContactIds?.Any() == true)
            {
                var existingContacts = await context.InteractionContacts
                   .Where(ic => ic.InteractionId == interaction.Id)
                   .ToListAsync();

                // Remove contacts not in the new list
                foreach (var contact in existingContacts.Where(ec => !model.ContactIds.Contains(ec.ContactId)))
                {
                    context.InteractionContacts.Remove(contact);
                }

                // Add new contacts
                foreach (var contactId in model.ContactIds.Except(existingContacts.Select(ec => ec.ContactId)))
                {
                    await context.InteractionContacts.AddAsync(new InteractionContact
                    {
                        InteractionId = interaction.Id,
                        ContactId = contactId
                    });
                }
            }

            // Process InteractionPartners
            if (model.PartnerIds?.Any() == true)
            {
                var existingPartners = await context.InteractionPartners
                    .Where(ip => ip.InteractionId == interaction.Id)
                    .ToListAsync();

                foreach (var partner in existingPartners.Where(ep => !model.PartnerIds.Contains(ep.PartnerId)))
                {
                    context.InteractionPartners.Remove(partner);
                }

                foreach (var partnerId in model.PartnerIds.Except(existingPartners.Select(ep => ep.PartnerId)))
                {
                    await context.InteractionPartners.AddAsync(new InteractionPartner
                    {
                        InteractionId = interaction.Id,
                        PartnerId = partnerId
                    });
                }
            }

            // Process InteractionUsers
            if (model.UserIds?.Any() == true)
            {
                var existingUsers = await context.InteractionUsers
                    .Where(iu => iu.InteractionId == interaction.Id)
                    .ToListAsync();

                foreach (var user in existingUsers.Where(eu => !model.UserIds.Contains(eu.UserId)))
                {
                    context.InteractionUsers.Remove(user);
                }

                foreach (var userId in model.UserIds.Except(existingUsers.Select(eu => eu.UserId)))
                {
                    await context.InteractionUsers.AddAsync(new InteractionUser
                    {
                        InteractionId = interaction.Id,
                        UserId = userId
                    });
                }
            }
            await context.SaveChangesAsync();
            await jtTransaction.CommitAsync();
        }
        catch
        {
            await jtTransaction.RollbackAsync();
            throw;
        }
    }

    public PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public PaginationResponse<InteractionModel> GetInteractionsWithSpecification(int userId, ISpecification<Interaction> specification, PaginationRequest pagination)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public async Task<InteractionModel?> GetInteraction(int userId, int id)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public IEnumerable<ExternalInteractionModel> GetPostedInteractions()
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public async Task<ExternalInteractionModel?> GetPostedInteraction(int id)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public async Task DeleteInteractionAsync(int userId, int id)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public async Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public PaginationResponse<InteractionModel> GetContactInteractionsAsync(int contactId, PaginationRequest request)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    // New secure methods - stub implementations for base class
    public virtual async Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var id) ? id : 0;
        
        return GetInteractions(userId, request);
    }
    
    public virtual async Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;
        
        return await GetInteraction(userId, id);
    }
    
    public virtual async Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var id) ? id : 0;
        
        return await UpdateInteractionAsync(userId, model);
    }
    
    public virtual async Task DeleteInteractionAsync(ClaimsPrincipal user, int id)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;
        
        await DeleteInteractionAsync(userId, id);
    }
}