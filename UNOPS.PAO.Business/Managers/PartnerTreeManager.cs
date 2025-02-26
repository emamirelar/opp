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

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeRequest model)
    {
        var entity = mapper.Map<PartnerTree>(model);

        await PartnerTreeRepository.AddAsync(entity);

        return mapper.Map<PartnerTreeModel>(entity);
    }

    public IEnumerable<PartnerTreeModel> GetPartnerTrees(int userId)
    {
        return PartnerTreeRepository
            .GetAll()
            .Select(mapper.Map<PartnerTreeModel>);
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

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, UpdatePartnerTreeRequest model)
    {
        var entity = await PartnerTreeRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map<UpdatePartnerTreeRequest, PartnerTree>(model, entity);

        await PartnerTreeRepository.UpdateAsync(entity);

        return mapper.Map<PartnerTreeModel>(entity);
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