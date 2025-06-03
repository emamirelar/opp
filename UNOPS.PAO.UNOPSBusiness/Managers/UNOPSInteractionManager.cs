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

    private async Task<InteractionModel> MapEntityToModelWithPermissionsAsync(UNOPSInteraction entity, IMapper mapper, ClaimsPrincipal user)
    {
        var result = MapEntityToModel(entity, mapper);
        
        // Add permissions if security service is available
        if (_securityService != null)
        {
            var permissions = await _securityService.GetEntityPermissionsAsync(entity, user);
            result.Permissions = new EntityPermissionsModel
            {
                CanRead = ((dynamic)permissions).canRead,
                CanUpdate = await _securityService.CanUserAccessEntityAsync(entity, user, "update"),
                CanDelete = await _securityService.CanUserAccessEntityAsync(entity, user, "delete"),
                CanCreate = await _securityService.CanUserAccessEntityAsync(entity, user, "create")
            };
        }
        
        return result;
    }

    private UNOPSInteraction MapModelToEntity(InteractionRequest model, UNOPSInteraction entity)
    {
        mapper.Map(model, entity);
        return entity;
    }

    private async Task<UNOPSInteraction> MapModelToEntity(InteractionRequest model)
    {
        var contact = await contactRepository.GetByIdAsync(model.ContactId);
        return MapModelToEntity(model, new UNOPSInteraction() { 
            ContactId = model.ContactId,
            Contact = contact ?? throw new BusinessException($"Contact {model.ContactId} not found"),
            Name = model.ContactId + " - " + model.Date
        });
    }

    private UNOPSInteraction MapModelToEntity(UpdateInteractionRequest model, UNOPSInteraction entity)
    {
        mapper.Map(model, entity);
        return entity;
    }

    public UNOPSInteractionManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IBusinessSecurityService securityService = null)
        : base(mapper, context, configuration)
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
            entity.Name = model.ContactId + " - " + model.Date;

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
            .Include(i => i.Contact)
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
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.Contact.Name)
                        : query.OrderByDescending(x => x.Contact.Name);
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
                nameof(Interaction.Contact),
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
            .GetAll()
            .Where(x => x.ContactId == contactId)
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

    // New secure methods with row-level filtering and permissions
    public async Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll(["Contact", "Contact.Partner", "Contact.Partner.PartnerOffice"])
            .AsQueryable();

        // Apply row-level security filters
        if (_securityService != null)
        {
            query = await _securityService.ApplyRowFiltersAsync(query, user, "read");
        }

        // Apply pagination with permissions
        var pagedResults = query.Paginate(
            x => MapEntityToModel(x, mapper),
            request
        );

        // Add permissions to each interaction
        if (_securityService != null)
        {
            var interactionsWithPermissions = new List<InteractionModel>();
            foreach (var interaction in pagedResults.Records)
            {
                var entity = await interactionRepository.GetByIdAsync(interaction.Id, ["Contact", "Contact.Partner", "Contact.Partner.PartnerOffice"]);
                if (entity != null)
                {
                    var interactionWithPermissions = await MapEntityToModelWithPermissionsAsync(entity, mapper, user);
                    interactionsWithPermissions.Add(interactionWithPermissions);
                }
            }
            
            return new PaginationResponse<InteractionModel>
            {
                Records = interactionsWithPermissions,
                TotalCount = pagedResults.TotalCount
            };
        }

        return pagedResults;
    }

    public async Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id)
    {
        var item = await interactionRepository.GetByIdAsync(id, ["Contact", "Contact.Partner", "Contact.Partner.PartnerOffice"]);
        if (item == null) return null;

        // Check if user can access this specific interaction
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(item, user, "read"))
        {
            return null; // User cannot access this interaction
        }

        return await MapEntityToModelWithPermissionsAsync(item, mapper, user);
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model)
    {
        var entity = await interactionRepository.GetByIdAsync(model.Id, ["Contact", "Contact.Partner", "Contact.Partner.PartnerOffice"]);
        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        // Check if user can update this interaction
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(entity, user, "update"))
        {
            throw new UnauthorizedAccessException("You don't have permission to update this interaction");
        }

        entity = MapModelToEntity(model, entity);

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

        // Update junction tables
        await ProcessJunctionTables(entity, model);

        await interactionRepository.UpdateAsync(entity);

        return await MapEntityToModelWithPermissionsAsync(entity, mapper, user);
    }

    public async Task DeleteInteractionAsync(ClaimsPrincipal user, int id)
    {
        var entity = await interactionRepository.GetByIdAsync(id, ["Contact", "Contact.Partner", "Contact.Partner.PartnerOffice"]);
        if (entity == null) return;

        // Check if user can delete this interaction
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(entity, user, "delete"))
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this interaction");
        }

        await interactionRepository.Delete(entity);
    }

    /// <summary>
    /// Gets comprehensive interaction details for AI prompts including all related entities
    /// </summary>
    /// <param name="id">The interaction ID</param>
    /// <returns>Complete interaction details with all relationships</returns>
    public async Task<InteractionModel?> GetInteractionDetailsAsync(int id)
    {
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                "Contact",
                "Contact.Partner",
                "Contact.Partner.PartnerOffice",
                "OrgUnit",
                "InteractionContacts",
                "InteractionPartners",
                "InteractionUsers",
                "InteractionContacts.Contact",
                "InteractionContacts.Contact.Partner",
                "InteractionPartners.Partner",
                "InteractionUsers.User",
                "Documents"
            });

        if (item == null)
        {
            return null;
        }

        // Map the entity to model with all relationships
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
    /// Gets comprehensive interaction details with security checks for AI prompts
    /// </summary>
    /// <param name="user">Current user claims</param>
    /// <param name="id">The interaction ID</param>
    /// <returns>Complete interaction details with all relationships and permissions</returns>
    public async Task<InteractionModel?> GetInteractionDetailsAsync(ClaimsPrincipal user, int id)
    {
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                "Contact",
                "Contact.Partner",
                "Contact.Partner.PartnerOffice",
                "OrgUnit",
                "InteractionContacts",
                "InteractionPartners", 
                "InteractionUsers",
                "InteractionContacts.Contact",
                "InteractionContacts.Contact.Partner",
                "InteractionPartners.Partner",
                "InteractionUsers.User",
                "Documents"
            });

        if (item == null) return null;

        // Check if user can access this specific interaction
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(item, user, "read"))
        {
            return null; // User cannot access this interaction
        }

        // Map the entity to model with permissions
        var result = await MapEntityToModelWithPermissionsAsync(item, mapper, user);
        
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
        return await GetInteractionDetailsAsync(entityId);
    }

    public virtual async Task<InteractionModel> FindGmailInteractionAsync(GmailInteractionRequest model)
    {
        var entity = await interactionRepository.GetAll().AsQueryable()
            .FirstOrDefaultAsync(x => x.GmailThreadId == model.GmailThreadId && !x.IsDeleted);

        return mapper.Map<InteractionModel>(entity);
    }

    public virtual async Task<InteractionModel?> CreateGmailInteractionAsync(InteractionRequest model)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if(model.EmailAddresses != null && model.EmailAddresses.Count > 0)
        {
            model.EmailAddresses = model.EmailAddresses.Distinct().ToList();
        }

        var entity = await MapModelToEntity(model);

        try
        {
            entity.Name = model.ContactId + " - " + model.Date;

            await interactionRepository.AddAsync(entity);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await ProcessGmailInteractionJunctionTables(entity, model);
        return mapper.Map<InteractionModel>(entity);
    }

    public virtual async Task<InteractionModel?> UpdateGmailInteractionAsync(UpdateInteractionRequest model)
    {
        var entity = await interactionRepository.GetByIdAsync(model.Id, ["Contact", "Contact.Partner", "Contact.Partner.PartnerOffice"]);
        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        if (model.EmailAddresses != null && model.EmailAddresses.Count > 0)
        {
            model.EmailAddresses = model.EmailAddresses.Distinct().ToList();
        }

        entity = MapModelToEntity(model, entity);

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

        // Update junction tables
        await ProcessGmailInteractionJunctionTables(entity, model);

        await interactionRepository.UpdateAsync(entity);

        return mapper.Map<InteractionModel>(entity);
    }

    private async Task ProcessGmailInteractionJunctionTables(Interaction interaction, InteractionRequest model)
    {
        await using var jtTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Process email-based Contact lookups
            if (model.EmailAddresses?.Any() == true)
            {
                var matchingContacts = await context.Contacts
                    .Where(c => model.EmailAddresses.Contains(c.Email))
                    .ToListAsync();

                var existingEmailContacts = await context.InteractionContacts
                    .Where(ic => ic.InteractionId == interaction.Id)
                    .Include(ic => ic.Contact)
                    .ToListAsync();

                // Add new contacts found by email
                foreach (var contact in matchingContacts)
                {
                    if (!existingEmailContacts.Any(ec => ec.ContactId == contact.Id))
                    {
                        await context.InteractionContacts.AddAsync(new InteractionContact
                        {
                            InteractionId = interaction.Id,
                            ContactId = contact.Id,
                            Contact = contact
                        });
                    }
                }
            }

            // Process email-based User lookups
            if (model.EmailAddresses?.Any() == true)
            {
                var matchingUsers = await context.GrantUsers
                    .Where(u => model.EmailAddresses.Contains(u.Email))
                    .ToListAsync();

                var existingEmailUsers = await context.InteractionUsers
                    .Where(iu => iu.InteractionId == interaction.Id)
                    .Include(iu => iu.User)
                    .ToListAsync();

                // Add new users found by email
                foreach (var user in matchingUsers)
                {
                    if (!existingEmailUsers.Any(eu => eu.UserId == user.Id))
                    {
                        await context.InteractionUsers.AddAsync(new InteractionUser
                        {
                            InteractionId = interaction.Id,
                            UserId = user.Id,
                            User = user
                        });
                    }
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
}