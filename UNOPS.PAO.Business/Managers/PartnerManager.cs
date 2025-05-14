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
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Specifications;

public class PartnerManager : IPartnerManager
{
    private IMapper mapper;

    private DataRepository<Partner> PartnerRepository;
    private DataRepository<OrganizationHierarchy> OrganizationHierarchyRepository;
    private DataRepository<PartnerCategory> PartnerCategoryRepository;

    public PartnerManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.PartnerRepository = new DataRepository<Partner>(context);
        this.OrganizationHierarchyRepository = new DataRepository<OrganizationHierarchy>(context);
        this.PartnerCategoryRepository = new DataRepository<PartnerCategory>(context);
    }

    public async Task<PartnerModel> CreatePartnerAsync(PartnerRequest model)
    {
        var entity = mapper.Map<Partner>(model);

        // Verify that the selected PartnerOffice is of type OrgUnit
        if (entity.PartnerOfficeId.HasValue)
        {
            var office = await OrganizationHierarchyRepository.GetByIdAsync(entity.PartnerOfficeId.Value);
            if (office == null || office.Type != OrganizationUnitType.OrgUnit)
            {
                throw new BusinessException("Partner Office must be of type OrgUnit");
            }
        }

        await PartnerRepository.AddAsync(entity);

        return mapper.Map<PartnerModel>(entity);
    }

    public PaginationResponse<PartnerModel> GetPartners(int userId, PaginationRequest request)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerCategory"])
            .Where(x => !x.IsDeleted && (x.PartnerOffice == null || x.PartnerOffice.Type == OrganizationUnitType.OrgUnit))
            .AsQueryable();

        return query.Paginate(
            x => mapper.Map<PartnerModel>(x),
            request
        );
    }
    
    public PaginationResponse<PartnerModel> GetPartnersWithSpecification(int userId, ISpecification<Partner> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = PartnerRepository.GetAll().AsQueryable();
        var filteredQuery = query.ApplySpecification(specification)
            .Where(x => x.PartnerOffice == null || x.PartnerOffice.Type == OrganizationUnitType.OrgUnit);
        
        // Apply pagination
        return filteredQuery.Paginate(
            x => mapper.Map<PartnerModel>(x),
            pagination
        );
    }

    public async Task<PartnerModel?> GetPartner(int userId, int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        if (item.PartnerCategoryId.HasValue)
        {
            var partnerCategory = await PartnerCategoryRepository.GetByIdAsync(item.PartnerCategoryId.Value);
            if (partnerCategory != null)
            {
                item.PartnerCategory = partnerCategory;
            }
        }
        if (item.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationHierarchyRepository
                .GetAll()
                .Where(x => x.Id == item.PartnerOfficeId.Value && x.Type == OrganizationUnitType.OrgUnit)
                .FirstOrDefaultAsync();
            if (partnerOffice != null)
            {
                item.PartnerOffice = partnerOffice;
            }
        }

        return mapper.Map<PartnerModel>(item);
    }

    /*public IEnumerable<ExternalPartnerModel> GetPostedPartners()
    {
        return PartnerRepository
            .GetAll()
            .Select(mapper.Map<ExternalPartnerModel>);
    }

    public async Task<ExternalPartnerModel?> GetPostedPartner(int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalPartnerModel>(item);
    }*/

    public async Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model)
    {
        var entity = await PartnerRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        // Verify that the selected PartnerOffice is of type OrgUnit
        if (model.PartnerOfficeId.HasValue)
        {
            var office = await OrganizationHierarchyRepository
                .GetAll()
                .Where(x => x.Id == model.PartnerOfficeId.Value && x.Type == OrganizationUnitType.OrgUnit)
                .FirstOrDefaultAsync();
            if (office == null)
            {
                throw new BusinessException("Partner Office must be of type OrgUnit");
            }
        }

        mapper.Map<UpdatePartnerRequest, Partner>(model, entity);
        await PartnerRepository.UpdateAsync(entity);

        return mapper.Map<PartnerModel>(entity);
    }

    /*public async Task<PartnerModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await PartnerRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        entity.Stage = newStage;

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

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

        var item = await PartnerRepository
            .GetAll(includes)
            .Where(x => x.Id == id && (x.PartnerOffice == null || x.PartnerOffice.Type == OrganizationUnitType.OrgUnit))
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return default;
        }

        var result = mapper.Map<PartnerModel>(item);
        return result;
    }

    public async Task<string?> UpdatePartnerLogoAsync(int partnerId, IFormFile file)
    {
        return null;
    }
}