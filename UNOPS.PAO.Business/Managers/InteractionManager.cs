namespace UNOPS.PAO.Business.Managers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;

public class InteractionManager : IInteractionManager
{
    private readonly IMapper mapper;
    private readonly DataRepository<Interaction> interactionRepository;
    private readonly AppDbContext context;

    public InteractionManager(IMapper mapper, AppDbContext context)
    {
        mapper = mapper;
        context = context;
        interactionRepository = new DataRepository<Interaction>(context);
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        var entity = mapper.Map<Interaction>(model);
        entity.Name = model.ContactId + " - " + model.Date;

        await interactionRepository.AddAsync(entity);

        return mapper.Map<InteractionModel>(entity);
    }

    public PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll()
            .AsQueryable();

        return query.Paginate(
            x => mapper.Map<InteractionModel>(x),
            request
        );
    }

    public PaginationResponse<InteractionModel> GetInteractionsWithSpecification(int userId, ISpecification<Interaction> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = interactionRepository.GetAll().AsQueryable();
        var filteredQuery = query.ApplySpecification(specification);
        
        // Apply pagination
        return filteredQuery.Paginate(
            x => mapper.Map<InteractionModel>(x),
            pagination
        );
    }

    public async Task<InteractionModel?> GetInteraction(int userId, int id)
    {
        var item = await interactionRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<InteractionModel>(item);
    }

    public IEnumerable<ExternalInteractionModel> GetPostedInteractions()
    {
        return interactionRepository
            .GetAll()
            .Select(mapper.Map<ExternalInteractionModel>);
    }

    public async Task<ExternalInteractionModel?> GetPostedInteraction(int id)
    {
        var item = await interactionRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalInteractionModel>(item);
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model)
    {
        var entity = await interactionRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map<UpdateInteractionRequest, Interaction>(model, entity);

        await interactionRepository.UpdateAsync(entity);

        return mapper.Map<InteractionModel>(entity);
    }

    public async Task DeleteInteractionAsync(int userId, int id)
    {
        var entity = await interactionRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await interactionRepository.Delete(entity);
        }
    }

    public async Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request)
    {
        var interaction = await interactionRepository.GetByIdAsync(id);
        if (interaction == null) {
            return null;
        }
        
        await interactionRepository.UpdateAsync(interaction);
        return mapper.Map<InteractionModel>(interaction);
    }

    public PaginationResponse<InteractionModel> GetContactInteractionsAsync(int contactId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll()
            .Where(x => x.ContactId == contactId)
            .OrderByDescending(x => x.Date)
            .AsQueryable();

        return query.Paginate(
            x => mapper.Map<InteractionModel>(x),
            request
        );
    }
} 