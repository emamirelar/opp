namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
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

public class UNOPSFundingOpportunityManager : IFundingOpportunityManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSFundingOpportunity> fundingOpportunityRepository;

    private DataRepository<SelectionMethodology> selectionMethodologyRepository;

    private CommonEntityRepository commonRepository;

    private string[] includes = ["Project", "Proposals", "SDGs", "SelectionMethodology", "EligibleEntities", "Countries", "Currency", "Documents"];

    private static FundingOpportunityModel MapEntityToModel(UNOPSFundingOpportunity entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSFundingOpportunity, FundingOpportunityModel>(entity);

        result.EligibleEntities = entity.EligibleEntities?.Select(mapper.Map<EligibleEntityModel>).ToList();
        result.Documents = entity.Documents?.Select(mapper.Map<DocumentModel>).ToList();
        result.Currency = entity.Currency != null ? mapper.Map<CurrencyModel>(entity.Currency) : null;

        ProjectModel project = mapper.Map<ProjectModel>(entity.Project);

        result.SDGs = entity.SDGs != null
            ? entity.SDGs.Select(x => mapper.Map<SDGModel>(x)).ToList()
            : [];

        result.SelectionMethodology = entity.SelectionMethodology != null
            ? mapper.Map<SelectionMethodology, SelectonMethodologyModel>(entity.SelectionMethodology)
            : null;

        result.Countries = entity.Countries != null
            ? entity.Countries.Select(x => mapper.Map<CountryModel>(x)).ToList()
            : [];

        var appType = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new ApplicationTypeModel() { Id = x.value.Name, DisplayName = x.attr?.Value })
            .Where(x => x.Id == entity.ApplicationTypeCode)
            .FirstOrDefault();

        result.ApplicationType = appType;

        result.Extensions.Add("project", project);

        return result;
    }
    private static ExternalFundingOpportunityModel MapEntityToExternalModel(UNOPSFundingOpportunity entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSFundingOpportunity, ExternalFundingOpportunityModel>(entity);

        result.EligibleEntities = entity.EligibleEntities?.Select(mapper.Map<EligibleEntityModel>).ToList();
        result.Documents = entity.Documents?.Select(mapper.Map<DocumentModel>).ToList();
        result.Currency = entity.Currency != null ? mapper.Map<CurrencyModel>(entity.Currency) : null;

        ProjectModel project = mapper.Map<ProjectModel>(entity.Project);

        result.SDGs = entity.SDGs != null
            ? entity.SDGs.Select(x => mapper.Map<SDGModel>(x)).ToList()
            : [];

        result.SelectionMethodology = entity.SelectionMethodology != null
            ? mapper.Map<SelectionMethodology, SelectonMethodologyModel>(entity.SelectionMethodology)
            : null;

        result.Countries = entity.Countries != null
            ? entity.Countries.Select(x => mapper.Map<CountryModel>(x)).ToList()
            : [];

        var appType = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new ApplicationTypeModel() { Id = x.value.Name, DisplayName = x.attr?.Value })
            .Where(x => x.Id == entity.ApplicationTypeCode)
            .FirstOrDefault();

        result.ApplicationType = appType;

        result.Extensions.Add("project", project);

        return result;
    }

    private UNOPSFundingOpportunity MapModelToEntity(FundingOpportunityRequest model, UNOPSFundingOpportunity entity)
    {
        mapper.Map(model, entity);

        entity.Currency = commonRepository.GetCurrencyByCodeAsync(model.CurrencyCode ?? string.Empty).Result;

        entity.SelectionMethodology = commonRepository.GetSelectionMethodologyByIdAsync(model.SelectionMethodologyId).Result;

        // Update Eligible Entities
        if (entity.EligibleEntities != null)
        {
            entity.EligibleEntities.Clear();
        }
        else
        {
            entity.EligibleEntities = [];
        }

        if (model.EligibleEntityIds != null)
        {
            var entities = (from x in commonRepository.GetEligibleEntities()
                            join id in model.EligibleEntityIds on x.Id equals id
                            select x
                            );

            foreach (var e in entities)
            {
                entity.EligibleEntities.Add(e);
            }
        }

        // Update SDGs
        if (entity.SDGs != null)
        {
            entity.SDGs.Clear();
        }
        else
        {
            entity.SDGs = [];
        }

        if (model.SDGIds != null)
        {
            var entities = (from x in commonRepository.GetSDGs()
                            join id in model.SDGIds on x.Id equals id
                            select x
                            );

            foreach (var e in entities)
            {
                entity.SDGs.Add(e);
            }
        }

        // Update Countries
        if (entity.Countries != null)
        {
            entity.Countries.Clear();
        }
        else
        {
            entity.Countries = [];
        }

        if (model.CountryIds != null)
        {
            var entities = (from x in commonRepository.GetCountries()
                            join id in model.CountryIds on x.Id equals id
                            select x
                            );

            foreach (var e in entities)
            {
                entity.Countries.Add(e);
            }
        }

        if (model.Extensions.TryGetValue("projectNumber", out object? number))
        {
            var project = commonRepository.GetProjectByNumberAsync(number?.ToString() ?? string.Empty).Result;

            if (project == null)
            {
                throw new BusinessException("A valid project is required");
            }

            entity.Project = project;
        }

        return entity;
    }

    private UNOPSFundingOpportunity MapModelToEntity(FundingOpportunityRequest model)
    {
        return MapModelToEntity(model, new UNOPSFundingOpportunity());
    }

    public UNOPSFundingOpportunityManager(IMapper mapper, UNOPSAppDbContext context)
    {
        this.mapper = mapper;
        fundingOpportunityRepository = new BaseRepository<UNOPSFundingOpportunity>(context);

        selectionMethodologyRepository = new DataRepository<SelectionMethodology>(context);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<FundingOpportunityModel> CreateFundingOpportunityAsync(FundingOpportunityRequest model)
    {
        var entity = MapModelToEntity(model);
        entity.SelectionMethodology = await selectionMethodologyRepository.GetByIdAsync(model.SelectionMethodologyId);

        await fundingOpportunityRepository.AddAsync(entity);

        return mapper.Map<FundingOpportunityModel>(entity);
    }

    public IEnumerable<FundingOpportunityModel> GetFundingOpportunities(int userId)
    {
        return fundingOpportunityRepository
            .GetAll(["SelectionMethodology"])
            .Select(x => MapEntityToModel(x, mapper));
    }
    
    public async Task<FundingOpportunityModel?> GetFundingOpportunity(int userId, int id)
    {
        var item = await fundingOpportunityRepository.GetByIdAsync(id, includes);
        if (item == null)
        {
            return default;
        }
        
        var docs = await fundingOpportunityRepository.GetDocumentsForEntityAsync(item.Id, DocumentParentEntityType.FundingOpportunity);
        item.Documents = docs.ToList();
        return MapEntityToModel(item, mapper);
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
            .GetAll(["SDGs", "Countries", "Currency"])
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalFundingOpportunityModel?> GetPostedFundingOpportunity(int id)
    {
        var item = await fundingOpportunityRepository.GetByIdAsync(id, ["EligibleEntities", "SDGs", "Documents", "Currency"]);

        if (item == null)
        {
            throw new BusinessException($"Funding opportunity {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    public async Task<FundingOpportunityModel?> UpdateFundingOpportunityAsync(int userId, UpdateFundingOpportunityRequest model)
    {
        var entity = await fundingOpportunityRepository.GetByIdAsync(model.Id, includes);

        if (entity == null)
        {
            throw new BusinessException($"Funding opportunity {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await fundingOpportunityRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    public async Task<FundingOpportunityModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await fundingOpportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        entity.Stage = newStage;

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