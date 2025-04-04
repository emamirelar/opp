namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

public class UNOPSPartnerTreeManager : IPartnerTreeManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSPartnerTree> partnerTreeRepository;

    private CommonEntityRepository commonRepository;

    //private string[] includes = ["Currency", "Documents"];

    private static PartnerTreeModel MapEntityToModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = new PartnerTreeModel
        {
            Data = mapper.Map<UNOPSPartnerTree, PartnerTreeDataModel>(entity)
        };

        return result;
    }
    private static ExternalPartnerTreeModel MapEntityToExternalModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartnerTree, ExternalPartnerTreeModel>(entity);

        return result;
    }

    private UNOPSPartnerTree MapModelToEntity(PartnerTreeRequest model, UNOPSPartnerTree entity)
    {
        mapper.Map(model, entity);

//        entity.Name = String.Concat(model.Name, ' ', model.Description, ' ', model.Code, ' ', model.LastName);

        return entity;
    }

    private UNOPSPartnerTree MapModelToEntity(PartnerTreeRequest model)
    {
        return MapModelToEntity(model, new UNOPSPartnerTree());
    }

    public UNOPSPartnerTreeManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        this.mapper = mapper;
        partnerTreeRepository = new BaseRepository<UNOPSPartnerTree>(context, configuration);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model)
    {
        var entity = mapper.Map<UNOPSPartnerTree>(model);
        
        await partnerTreeRepository.AddAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    public IEnumerable<PartnerTreeModel> GetPartnerTrees(int userId, string sortBy = "Name", bool ascending = true)
    {
        var allTrees = partnerTreeRepository
            .GetAllSortedAsync(sortBy, ascending)
            .Result
            .Select(x => MapEntityToModel(x, mapper))
            .ToList();

        var lookup = allTrees.ToLookup(x => x.Data.Parent);
        return BuildHierarchy(lookup, string.Empty);
    }

    private IEnumerable<PartnerTreeModel> BuildHierarchy(ILookup<string, PartnerTreeModel> lookup, string parentCode)
    {
        foreach (var item in lookup[parentCode])
        {
            item.Children = BuildHierarchy(lookup, item.Data.Code).ToList();
            yield return item;
        }
    }

    public async Task<PartnerTreeModel?> GetPartnerTree(int userId, int id)
    {
        var item = await partnerTreeRepository.GetByIdAsync(id);
        if (item == null)
        {
            return default;
        }

        return MapEntityToModel(item, mapper);
    }

    /*public async Task<string?> GetContactStage(int id)
    {
        var item = await contactRepository.GetByIdAsync(id);

        if (item == null)
        {
            return null;
        }

        return item.Stage;
    }*/

    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return partnerTreeRepository
            .GetAll()
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await partnerTreeRepository.GetByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner Level {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model)
    {
        var entity = await partnerTreeRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner Level {model.Id} does not exist.");
        }

        mapper.Map(model, entity);

        await partnerTreeRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    /*public async Task<ContactModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await contactRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        entity.Stage = newStage;

        await contactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }*/

    public async Task DeletePartnerTreeAsync(int userId, int id)
    {
        var entity = await partnerTreeRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await partnerTreeRepository.Delete(entity);
        }
    }
}