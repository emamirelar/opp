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

public class UNOPSInteractionManager : IInteractionManager
{
    private readonly IMapper mapper;
    private readonly BaseRepository<UNOPSInteraction> interactionRepository;
    private readonly BaseRepository<UNOPSContact> contactRepository;
    private readonly UNOPSAppDbContext context;

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

    public UNOPSInteractionManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        this.mapper = mapper;
        this.context = context;
        interactionRepository = new BaseRepository<UNOPSInteraction>(context, configuration);
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        var entity = await MapModelToEntity(model);
        entity.Name = model.ContactId + " - " + model.Date;

        await interactionRepository.AddAsync(entity);

        await context.SaveChangesAsync();

        foreach (var contactId in model.ContactIds)
        {
            await context.InteractionContacts.AddAsync(new InteractionContact
            {
                InteractionId = entity.Id,
                ContactId = contactId
            });
        }

        foreach (var partnerId in model.PartnerIds)
        {
            await context.InteractionPartners.AddAsync(new InteractionPartner
            {
                InteractionId = entity.Id,
                PartnerId = partnerId
            });
        }

        foreach (var userId in model.UserIds)
        {
            await context.InteractionUsers.AddAsync(new InteractionUser
            {
                InteractionId = entity.Id,
                UserId = userId
            });
        }

        return mapper.Map<InteractionModel>(entity);
    }

    private async Task ProcessJunctionTables(Interaction interaction, InteractionRequest model)
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
        //await context.SaveChangesAsync();
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
                case "data":
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.Data)
                        : query.OrderByDescending(x => x.Data);
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
        /*var item = await interactionRepository.GetByIdAsync(id);
        if (item == null || item.IsDeleted)
        {
            return default;
        }

        return MapEntityToModel(item, mapper);*/
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
        /*var entity = await interactionRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await interactionRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);*/
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
} 