namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.Extensions.Configuration;

public class UNOPSInteractionManager : IInteractionManager
{
    private readonly IMapper mapper;
    private readonly BaseRepository<UNOPSInteraction> interactionRepository;
    private readonly BaseRepository<UNOPSContact> contactRepository;
    private readonly CommonEntityRepository commonRepository;

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
        interactionRepository = new BaseRepository<UNOPSInteraction>(context, configuration);
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        var entity = await MapModelToEntity(model);
        await interactionRepository.AddAsync(entity);
        return mapper.Map<InteractionModel>(entity);
    }
    public PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll()
            .AsQueryable()
            .Include(i => i.Contact)
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
        var item = await interactionRepository.GetByIdAsync(id);
        if (item == null || item.IsDeleted)
        {
            return default;
        }

        return MapEntityToModel(item, mapper);
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model)
    {
        var entity = await interactionRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await interactionRepository.UpdateAsync(entity);

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
} 