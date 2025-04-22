using Microsoft.AspNetCore.Http;

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
using UNOPS.PAO.Utilities.Helpers;

public class UNOPSPartnerManager : IPartnerManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSPartner> PartnerRepository;
    private BaseRepository<UNOPSOrganizationUnit> OrganizationUnitRepository;
    private BaseRepository<UNOPSPartnerCategory> PartnerCategoryRepository;

    private CommonEntityRepository commonRepository;


    private GoogleCloudStorageService GoogleCloudStorageService;

    //private string[] includes = ["Currency", "Documents"];

    private static PartnerModel MapEntityToModel(UNOPSPartner entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartner, PartnerModel>(entity);

        /*result.EligibleEntities = entity.EligibleEntities?.Select(mapper.Map<EligibleEntityModel>).ToList();
        
        var appType = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new ApplicationTypeModel() { Id = x.value.Name, DisplayName = x.attr?.Value })
            .Where(x => x.Id == entity.ApplicationTypeCode)
            .FirstOrDefault();

        result.ApplicationType = appType;*/

        //result.Extensions.Add("project", project);

        return result;
    }
    /*private static ExternalPartnerModel MapEntityToExternalModel(UNOPSPartner entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartner, ExternalPartnerModel>(entity);

        *//*result.EligibleEntities = entity.EligibleEntities?.Select(mapper.Map<EligibleEntityModel>).ToList();
        
        var appType = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new ApplicationTypeModel() { Id = x.value.Name, DisplayName = x.attr?.Value })
            .Where(x => x.Id == entity.ApplicationTypeCode)
            .FirstOrDefault();

        result.ApplicationType = appType;*//*

        //result.Extensions.Add("project", project);

        return result;
    }*/

    private UNOPSPartner MapModelToEntity(PartnerRequest model, UNOPSPartner entity)
    {
        mapper.Map(model, entity);
        // Update Eligible Entities
        /*if (entity.EligibleEntities != null)
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
        }*/

        return entity;
    }

    private UNOPSPartner MapModelToEntity(PartnerRequest model)
    {
        return MapModelToEntity(model, new UNOPSPartner());
    }

    public UNOPSPartnerManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        this.mapper = mapper;
        PartnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        OrganizationUnitRepository = new BaseRepository<UNOPSOrganizationUnit>(context, configuration);
        PartnerCategoryRepository = new BaseRepository<UNOPSPartnerCategory>(context, configuration);
        
        GoogleCloudStorageService = new GoogleCloudStorageService(configuration);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<PartnerModel> CreatePartnerAsync(PartnerRequest model)
    {
        var entity = MapModelToEntity(model);

        await PartnerRepository.AddAsync(entity);

        return mapper.Map<PartnerModel>(entity);
    }

    public PaginationResponse<PartnerModel> GetPartners(int userId, PaginationRequest request)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerCategory"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        return query.Paginate(
            x => MapEntityToModel(x, mapper),
            request
        );
    }

    public async Task<PartnerModel?> GetPartner(int userId, int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);
        if (item == null)
        {
            return default;
        }

        if(item.PartnerCategoryId.HasValue)
        {
            var partnerCategory = await PartnerCategoryRepository.GetByIdAsync(item.PartnerCategoryId.Value);
            if (partnerCategory != null)
            {
                item.PartnerCategory = partnerCategory;
            }
        }
        if (item.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationUnitRepository.GetByIdAsync(item.PartnerOfficeId.Value);
            if (partnerOffice != null)
            {
                item.PartnerOffice = partnerOffice;
            }
        }

        return MapEntityToModel(item, mapper);
    }

    /*public async Task<string?> GetPartnerStage(int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);

        if (item == null)
        {
            return null;
        }

        return item.Stage;
    }*/

    /*public IEnumerable<ExternalPartnerModel> GetPostedPartners()
    {
        return PartnerRepository
            .GetAll()
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalPartnerModel?> GetPostedPartner(int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }*/

    public async Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model)
    {
        var entity = await PartnerRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await PartnerRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    /*public async Task<PartnerModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await PartnerRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        entity.Stage = newStage;

        await PartnerRepository.UpdateAsync(entity);

        return mapper.Map<PartnerModel>(entity);
    }*/

    public async Task DeletePartnerAsync(int userId, int id)
    {
        var entity = await PartnerRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await PartnerRepository.Delete(entity);
        }
    }
    public async Task<PartnerModel?> GetPartnerAsync(int id)
    {
        string[] includes = ["Documents", "PartnerOffice", "PartnerCategory"];

        var item = await PartnerRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        var result = mapper.Map<PartnerModel>(item);

        //result.ApplicationType = applicationTypeManager.GetApplicationTypeByCode(item.ApplicationTypeCode);

        return result;
    }
    
    
    public async Task<string?> UpdatePartnerLogoAsync(int partnerId, IFormFile file)
    {
        var entity = await PartnerRepository.GetByIdAsync(partnerId);

        if (entity == null)
        {
            throw new BusinessException($"Partner {partnerId} does not exist.");
        }

        // Upload file to Google Cloud Storage
        string imageUrl = await GoogleCloudStorageService.UploadFileToGCS(file);
        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new BusinessException("Failed to upload the image to cloud storage");
        }

        entity.LogoUrl = imageUrl;
        await PartnerRepository.UpdateAsync(entity);

        return entity.LogoUrl;
    }
}