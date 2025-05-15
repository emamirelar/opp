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
using System.Security.Claims;

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

        return mapper.Map<PartnerModel>(item);
    }

    public async Task<string?> UpdatePartnerLogoAsync(int partnerId, IFormFile file)
    {
        var entity = await PartnerRepository.GetByIdAsync(partnerId);
        if (entity == null)
        {
            return null;
        }

        // Save the file to a location and get its URL
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"partner-{partnerId}-logo-{DateTime.UtcNow.Ticks}{fileExtension}";
        var filePath = Path.Combine("wwwroot", "uploads", "partners", fileName);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Save the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update the entity with the logo URL (relative path)
        entity.LogoUrl = $"/uploads/partners/{fileName}";
        await PartnerRepository.UpdateAsync(entity);

        // Return the URL
        return entity.LogoUrl;
    }
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the partner
    /// </summary>
    public async Task<bool> HasPermissionAsync(int userId, int partnerId, string operation)
    {
        // Get the partner entity
        var entity = await PartnerRepository.GetByIdAsync(partnerId);
        if (entity == null)
        {
            return false;
        }
        
        // Basic permission rules:
        // 1. Administrator can do anything
        // 2. Creator of the partner can do anything with their own partners
        // 3. For Read operations, any Internal or Partner role can access
        // 4. For Update/Delete, only creator or admin can perform
        
        // Check if user is the creator
        bool isCreator = entity.CreatedBy == userId;
        
        // If user is creator, they have full access
        if (isCreator)
        {
            return true;
        }
        
        // For Read operations, allow access to all users with Partner role or higher
        if (operation == "Read")
        {
            // This simplified check just allows reading for almost all users
            // In a real implementation, you'd check against user roles in a database
            return true;
        }
        
        // For other operations (Update, Delete), only allow if user is creator or has admin privileges
        // This simplified version just denies access to non-creators
        // In a real implementation, you'd check if the user has Administrator role
        return false;
    }
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the partner
    /// </summary>
    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, int partnerId, string operation)
    {
        // Get user ID from claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return false;
        }
        
        // Use the existing method
        return await HasPermissionAsync(userId, partnerId, operation);
    }
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the partner
    /// </summary>
    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, Partner partner, string operation)
    {
        // Get user ID from claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return false;
        }
        
        // Check if user is the creator
        bool isCreator = partner.CreatedBy == userId;
        
        // If user is creator, they have full access
        if (isCreator)
        {
            return true;
        }
        
        // Check if user is administrator
        bool isAdmin = user.IsInRole("Administrator");
        if (isAdmin)
        {
            return true;
        }
        
        // For Read operations, allow access to all users with Partner role or higher
        if (operation == "Read")
        {
            return user.IsInRole("Partner") || user.IsInRole("Internal");
        }
        
        // For other operations (Update, Delete), only allow if user is creator or has admin privileges
        return false;
    }
}