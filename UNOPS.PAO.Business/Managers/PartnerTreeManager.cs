namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class PartnerTreeManager : IPartnerTreeManager
{
    private IMapper mapper;

    private DataRepository<PartnerTree> PartnerTreeRepository;

    public PartnerTreeManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.PartnerTreeRepository = new DataRepository<PartnerTree>(context);
    }

    private PartnerTreeModel MapEntityToModel(PartnerTree entity)
    {
        var result = new PartnerTreeModel
        {
            Data = mapper.Map<PartnerTree, PartnerTreeDataModel>(entity)
        };

        return result;
    }

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model)
    {
        var entity = mapper.Map<PartnerTree>(model);

        await PartnerTreeRepository.AddAsync(entity);

        return MapEntityToModel(entity);
    }

    public IEnumerable<PartnerTreeModel> GetPartnerTrees(int userId, string sortBy = "Name", bool ascending = true)
    {
        var allTrees = PartnerTreeRepository
            .GetAllSortedAsync(sortBy, ascending)
            .Result
            .Select(x => MapEntityToModel(x))
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
        var item = await PartnerTreeRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<PartnerTreeModel>(item);
    }

    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return PartnerTreeRepository
            .GetAll()
            .Select(mapper.Map<ExternalPartnerTreeModel>);
    }

    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await PartnerTreeRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalPartnerTreeModel>(item);
    }

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model)
    {
        var entity = await PartnerTreeRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map(model, entity);

        await PartnerTreeRepository.UpdateAsync(entity);

        return MapEntityToModel(entity);
    }

    public async Task DeletePartnerTreeAsync(int userId, int id)
    {
        var entity = await PartnerTreeRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await PartnerTreeRepository.Delete(entity);
        }
    }
}