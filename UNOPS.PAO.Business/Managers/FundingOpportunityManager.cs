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

public class FundingOpportunityManager : IFundingOpportunityManager
{
    private IMapper mapper;

    private DataRepository<FundingOpportunity> fundingOpportunityRepository;
    private DataRepository<SelectionMethodology> selectionMethodologyRepository;

    public FundingOpportunityManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.fundingOpportunityRepository = new DataRepository<FundingOpportunity>(context);
        this.selectionMethodologyRepository = new DataRepository<SelectionMethodology>(context);
    }

    public async Task<FundingOpportunityModel> CreateFundingOpportunityAsync(FundingOpportunityRequest model)
    {
        var entity = mapper.Map<FundingOpportunity>(model);

        entity.SelectionMethodology = await selectionMethodologyRepository.GetByIdAsync(model.SelectionMethodologyId);
        
        await fundingOpportunityRepository.AddAsync(entity);

        return mapper.Map<FundingOpportunityModel>(entity);
    }

    public IEnumerable<FundingOpportunityModel> GetFundingOpportunities(int userId)
    {
        return fundingOpportunityRepository
            .GetAll()
            .Select(mapper.Map<FundingOpportunityModel>);
    }

    public async Task<FundingOpportunityModel?> GetFundingOpportunity(int userId, int id)
    {
        var item = await fundingOpportunityRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<FundingOpportunityModel>(item);
    }

    public async Task<string?> GetFundingOpportunityStage(int id)
    {
        var item = await fundingOpportunityRepository.GetByIdAsync(id);

        if (item == null)
        {
            return null;
        }

        return item.Stage;
    }

    public IEnumerable<ExternalFundingOpportunityModel> GetPostedFundingOpportunities()
    {
        return fundingOpportunityRepository
            .GetAll()
            .Select(mapper.Map<ExternalFundingOpportunityModel>);
    }

    public async Task<ExternalFundingOpportunityModel?> GetPostedFundingOpportunity(int id)
    {
        var item = await fundingOpportunityRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalFundingOpportunityModel>(item);
    }

    public async Task<FundingOpportunityModel?> UpdateFundingOpportunityAsync(int userId, UpdateFundingOpportunityRequest model)
    {
        var entity = await fundingOpportunityRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map<UpdateFundingOpportunityRequest, FundingOpportunity>(model, entity);

        await fundingOpportunityRepository.UpdateAsync(entity);

        return mapper.Map<FundingOpportunityModel>(entity);
    }

    public async Task<FundingOpportunityModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await fundingOpportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        entity.Stage = newStage;

        if (newStage == "Open")
        { 
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        await fundingOpportunityRepository.UpdateAsync(entity);

        return mapper.Map<FundingOpportunityModel>(entity);
    }

    public async Task DeleteFundingOpportunityAsync(int userId, int id)
    {
        var entity = await fundingOpportunityRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await fundingOpportunityRepository.Delete(entity);
        }
    }
}