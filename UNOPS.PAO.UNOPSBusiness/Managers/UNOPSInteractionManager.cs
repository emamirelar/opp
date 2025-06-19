namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Business.Repositories.Generic;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Attributes;

public class UNOPSInteractionManager : BaseUNOPSManager, IInteractionManager
{
    private readonly IMapper mapper;
    private readonly BaseRepository<UNOPSInteraction> interactionRepository;
    private readonly BaseRepository<UNOPSContact> contactRepository;
    private readonly UNOPSAppDbContext context;
    private readonly IBusinessSecurityService _securityService;

    private static InteractionModel MapEntityToModel(UNOPSInteraction entity, IMapper mapper)
    {
        return mapper.Map<UNOPSInteraction, InteractionModel>(entity);
    }



    private UNOPSInteraction MapModelToEntity(InteractionRequest model, UNOPSInteraction entity)
    {
        mapper.Map(model, entity);
        return entity;
    }

    private async Task<UNOPSInteraction> MapModelToEntity(InteractionRequest model)
    {
        // Contact relationships are now handled through InteractionContacts many-to-many table
        var contactId = model.ContactIds?.FirstOrDefault() ?? 0;
        
        return MapModelToEntity(model, new UNOPSInteraction() { 
            Name = contactId + " - " + model.Date
        });
    }

    private UNOPSInteraction MapModelToEntity(UpdateInteractionRequest model, UNOPSInteraction entity)
    {
        mapper.Map(model, entity);
        return entity;
    }

    public UNOPSInteractionManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IBusinessSecurityService securityService = null)
        : base(mapper, context, configuration, null, securityService)
    {
        this.mapper = mapper;
        this.context = context;
        interactionRepository = new BaseRepository<UNOPSInteraction>(context, configuration);
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        _securityService = securityService;
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var entity = await MapModelToEntity(model);

        try
        {
            // Use first contact ID for naming, or default to interaction date
            var contactId = model.ContactIds?.FirstOrDefault() ?? 0;
            entity.Name = contactId + " - " + model.Date;

            await interactionRepository.AddAsync(entity);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await ProcessJunctionTables(entity, model);
        return mapper.Map<InteractionModel>(entity);
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
        var query = interactionRepository
            .GetAll()
            .AsQueryable()
            .Include(i => i.OrgUnit)
            .Include(i => i.InteractionContacts).ThenInclude(ic => ic.Contact)
            .Include(i => i.InteractionPartners).ThenInclude(ip => ip.Partner)
            .Include(i => i.InteractionUsers).ThenInclude(iu => iu.User)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            switch (request.OrderBy.ToLower())
            {
                case "type":
                    query = request.Ascending ?? true 
                        ? query.OrderBy(x => x.Type)
                        : query.OrderByDescending(x => x.Type);
                    break;
                case "contactid":
                    // Order by first contact name through InteractionContacts
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.InteractionContacts.FirstOrDefault().Contact.Name)
                        : query.OrderByDescending(x => x.InteractionContacts.FirstOrDefault().Contact.Name);
                    break;
                case "date":
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.Date)
                        : query.OrderByDescending(x => x.Date);
                    break;
                case "description":
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.Description)
                        : query.OrderByDescending(x => x.Description);
                    break;
                default:
                    query = query.OrderByDescending(x => x.Date);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.Date);
        }

        return query.Paginate(
            x => mapper.Map<InteractionModel>(x),
            request
        );
    }

    public async Task<InteractionModel?> GetInteraction(int userId, int id)
    {
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                nameof(Interaction.OrgUnit),
                nameof(Interaction.InteractionContacts),
                nameof(Interaction.InteractionPartners),
                nameof(Interaction.InteractionUsers),
                $"{nameof(Interaction.InteractionContacts)}.{nameof(InteractionContact.Contact)}",
                $"{nameof(Interaction.InteractionPartners)}.{nameof(InteractionPartner.Partner)}",
                $"{nameof(Interaction.InteractionUsers)}.{nameof(InteractionUser.User)}"
            });

        if (item == null)
        {
            return default;
        }

        InteractionModel retVal = MapEntityToModel(item, mapper);

        foreach (var contact in item.InteractionContacts)
        {
            retVal.ContactIds.Add(contact.ContactId);
        }

        foreach (var partner in item.InteractionPartners)
        {
            retVal.PartnerIds.Add(partner.PartnerId);
        }

        foreach (var user in item.InteractionUsers)
        {
            retVal.UserIds.Add(user.UserId);
        }

        return retVal;
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model)
    {
        var entity = await interactionRepository.GetByIdAsync(model.Id,
            includes: new[]
            {
                nameof(Interaction.InteractionContacts),
                nameof(Interaction.InteractionPartners),
                nameof(Interaction.InteractionUsers)
            });

        if (entity == null) return null;

        mapper.Map(model, entity);

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

        // Update junction tables
        await ProcessJunctionTables(entity, model);

        await interactionRepository.UpdateAsync(entity);
        //return mapper.Map<InteractionModel>(entity);
        return MapEntityToModel(entity, mapper);
    }

    public async Task DeleteInteractionAsync(int userId, int id)
    {
        var entity = await interactionRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await interactionRepository.Delete(entity);
        }
    }
    
    public PaginationResponse<InteractionModel> GetContactInteractionsAsync(int contactId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll(["InteractionContacts"])
            .Where(x => x.InteractionContacts.Any(ic => ic.ContactId == contactId))
            .OrderByDescending(x => x.Date)
            .AsQueryable()
            .Where(x => !x.IsDeleted);

        return query.Paginate(
            x => mapper.Map<InteractionModel>(x),
            request
        );
    }

    public PaginationResponse<InteractionModel> GetInteractionsWithSpecification(int userId, ISpecification<Interaction> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = interactionRepository.GetAll().AsQueryable()
            .Where(x => !x.IsDeleted);
        var filteredQuery = query.ApplySpecification(specification);
        
        // Apply pagination
        return filteredQuery.Paginate(
            x => mapper.Map<InteractionModel>(x),
            pagination
        );
    }

    public IEnumerable<ExternalInteractionModel> GetPostedInteractions()
    {
        // Implementation for external interactions if needed
        return new List<ExternalInteractionModel>();
    }

    public async Task<ExternalInteractionModel?> GetPostedInteraction(int id)
    {
        // Implementation for external interaction by id if needed
        return null;
    }

    public async Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request)
    {
        var entity = await interactionRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new BusinessException($"Interaction {id} does not exist.");
        }

        entity = MapModelToEntity(request, entity);
        await interactionRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    /// <summary>
    /// Gets all interactions with row-level security applied
    /// </summary>
    [RBAC("read", Entity = "Interaction", ApplyRowFiltering = true, ApplyColumnFiltering = true)]
    public async Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        // RBAC interceptor handles security enforcement
        var query = interactionRepository
            .GetAll(["InteractionContacts", "InteractionContacts.Contact", "InteractionContacts.Contact.Partner", "InteractionContacts.Contact.Partner.PartnerOffice"])
            .AsQueryable();

        var interactions = query.Paginate(
            x => MapEntityToModel(x, mapper),
            request
        );

        // Add permissions for frontend UI
        foreach (var interaction in interactions.Records)
        {
            var entity = await interactionRepository.GetByIdAsync(interaction.Id, ["InteractionContacts", "InteractionContacts.Contact", "InteractionContacts.Contact.Partner", "InteractionContacts.Contact.Partner.PartnerOffice"]);
            if (entity != null)
            {
                interaction.Permissions = await GetEntityPermissionsAsync(entity, user);
            }
        }

        return interactions;
    }

    /// <summary>
    /// Gets a specific interaction with row-level security applied
    /// </summary>
    [RBAC("read", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "id", ApplyColumnFiltering = true)]
    public async Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var item = await interactionRepository.GetByIdAsync(id, ["InteractionContacts", "InteractionContacts.Contact", "InteractionContacts.Contact.Partner", "InteractionContacts.Contact.Partner.PartnerOffice"]);
        if (item == null) return null;

        var result = MapEntityToModel(item, mapper);
        
        // Add permissions for frontend UI
        result.Permissions = await GetEntityPermissionsAsync(item, user);

        return result;
    }

    /// <summary>
    /// Updates an interaction with permission validation
    /// </summary>
    [RBAC("update", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "model.Id")]
    public async Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model)
    {
        // RBAC interceptor handles security enforcement
        var entity = await interactionRepository.GetByIdAsync(model.Id, ["InteractionContacts", "InteractionContacts.Contact", "InteractionContacts.Contact.Partner", "InteractionContacts.Contact.Partner.PartnerOffice"]);
        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

        // Update junction tables
        await ProcessJunctionTables(entity, model);

        await interactionRepository.UpdateAsync(entity);

        var result = MapEntityToModel(entity, mapper);
        
        // Add permissions for frontend UI
        result.Permissions = await GetEntityPermissionsAsync(entity, user);

        return result;
    }

    /// <summary>
    /// Deletes an interaction with permission validation
    /// </summary>
    [RBAC("delete", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "id")]
    public async Task DeleteInteractionAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var entity = await interactionRepository.GetByIdAsync(id, ["InteractionContacts", "InteractionContacts.Contact", "InteractionContacts.Contact.Partner", "InteractionContacts.Contact.Partner.PartnerOffice"]);
        if (entity == null) return;

        await interactionRepository.Delete(entity);
    }

    /// <summary>
    /// Gets comprehensive interaction details with security checks for AI prompts
    /// </summary>
    [RBAC("read", Entity = "Interaction", RequireEntityAccess = true, EntityIdParameterName = "id", ApplyColumnFiltering = true)]
    public async Task<InteractionModel?> GetInteractionDetailsAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                "OrgUnit",
                "InteractionContacts",
                "InteractionPartners", 
                "InteractionUsers",
                "InteractionContacts.Contact",
                "InteractionContacts.Contact.Partner",
                "InteractionContacts.Contact.Partner.PartnerOffice",
                "InteractionPartners.Partner",
                "InteractionUsers.User",
                "Documents"
            });

        if (item == null) return null;

        var result = MapEntityToModel(item, mapper);
        
        // Populate junction table IDs
        if (item.InteractionContacts != null)
        {
            result.ContactIds = item.InteractionContacts.Select(ic => ic.ContactId).ToList();
        }

        if (item.InteractionPartners != null)
        {
            result.PartnerIds = item.InteractionPartners.Select(ip => ip.PartnerId).ToList();
        }

        if (item.InteractionUsers != null)
        {
            result.UserIds = item.InteractionUsers.Select(iu => iu.UserId).ToList();
        }

        // Add permissions for frontend UI
        result.Permissions = await GetEntityPermissionsAsync(item, user);

        return result;
    }

    /// <summary>
    /// Gets comprehensive interaction details for AI prompts including all related entities (legacy method without security)
    /// </summary>
    public async Task<InteractionModel?> GetInteractionDetailsAsync(int id)
    {
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                "OrgUnit",
                "InteractionContacts",
                "InteractionPartners",
                "InteractionUsers",
                "InteractionContacts.Contact",
                "InteractionContacts.Contact.Partner",
                "InteractionContacts.Contact.Partner.PartnerOffice",
                "InteractionPartners.Partner",
                "InteractionUsers.User",
                "Documents"
            });

        if (item == null)
        {
            return null;
        }

        var result = MapEntityToModel(item, mapper);
        
        // Populate junction table IDs
        if (item.InteractionContacts != null)
        {
            result.ContactIds = item.InteractionContacts.Select(ic => ic.ContactId).ToList();
        }

        if (item.InteractionPartners != null)
        {
            result.PartnerIds = item.InteractionPartners.Select(ip => ip.PartnerId).ToList();
        }

        if (item.InteractionUsers != null)
        {
            result.UserIds = item.InteractionUsers.Select(iu => iu.UserId).ToList();
        }

        return result;
    }

    /// <summary>
    /// Implementation of abstract method from BaseUNOPSManager
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        if (user != null)
        {
            return await GetInteractionDetailsAsync(user, entityId);
        }
        else
        {
            return await GetInteractionDetailsAsync(entityId);
        }
    }
} 