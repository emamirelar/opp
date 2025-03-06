namespace UNOPS.PAO.Business.Managers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;

public class InteractionManager : IInteractionManager
{
    private IMapper mapper;

    private DataRepository<Interaction> InteractionRepository;

    public InteractionManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.InteractionRepository = new DataRepository<Interaction>(context);
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        var entity = mapper.Map<Interaction>(model);
        entity.Name = model.ContactId + " - " + model.Date;

        await InteractionRepository.AddAsync(entity);

        return mapper.Map<InteractionModel>(entity);
    }

    public PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request)
    {
        var query = InteractionRepository
            .GetAll()
            .AsQueryable();

        return query.Paginate(
            x => mapper.Map<InteractionModel>(x),
            request
        );
    }

    public async Task<InteractionModel?> GetInteraction(int userId, int id)
    {
        var item = await InteractionRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<InteractionModel>(item);
    }

    public IEnumerable<ExternalInteractionModel> GetPostedInteractions()
    {
        return InteractionRepository
            .GetAll()
            .Select(mapper.Map<ExternalInteractionModel>);
    }

    public async Task<ExternalInteractionModel?> GetPostedInteraction(int id)
    {
        var item = await InteractionRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalInteractionModel>(item);
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model)
    {
        var entity = await InteractionRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map<UpdateInteractionRequest, Interaction>(model, entity);

        await InteractionRepository.UpdateAsync(entity);

        return mapper.Map<InteractionModel>(entity);
    }

    public async Task DeleteInteractionAsync(int userId, int id)
    {
        var entity = await InteractionRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await InteractionRepository.Delete(entity);
        }
    }

    public async Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request)
    {
        var interaction = await InteractionRepository.GetByIdAsync(id);
        if (interaction == null) {
            return null;
        }
        
        await InteractionRepository.UpdateAsync(interaction);
        return mapper.Map<InteractionModel>(interaction);
    }
} 