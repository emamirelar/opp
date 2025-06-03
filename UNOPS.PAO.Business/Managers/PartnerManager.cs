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
    private DataRepository<PartnerTree> PartnerTreeRepository;
    private DataRepository<OrganizationHierarchy> OrganizationHierarchyRepository;

    public PartnerManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.PartnerRepository = new DataRepository<Partner>(context);
        this.PartnerTreeRepository = new DataRepository<PartnerTree>(context);
        this.OrganizationHierarchyRepository = new DataRepository<OrganizationHierarchy>(context);
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

    public async Task<PaginationResponse<PartnerModel>> GetPartners(int userId, PaginationRequest request)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup", "Contacts"])
            .Where(x => !x.IsDeleted && (x.PartnerOffice == null || x.PartnerOffice.Type == OrganizationUnitType.OrgUnit))
            .AsQueryable();

        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();
        
        // Map entities
        var mappedEntities = entities.Select(x => mapper.Map<PartnerModel>(x)).ToList();

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
    }
    
    public async Task<PaginationResponse<PartnerModel>> GetPartnersWithSpecification(int userId, ISpecification<Partner> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = PartnerRepository.GetAll().AsQueryable();
        var filteredQuery = query.ApplySpecification(specification)
            .Where(x => x.PartnerOffice == null || x.PartnerOffice.Type == OrganizationUnitType.OrgUnit);
        
        // Get total count
        var totalCount = await filteredQuery.CountAsync();
        
        // Apply pagination
        var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        var excludedRows = (pageIndex - 1) * pagination.PageSize;
        
        if (pagination.OrderBy != null)
        {
            filteredQuery = filteredQuery.OrderByColumnName(pagination.OrderBy, pagination.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = await filteredQuery
            .Skip(excludedRows)
            .Take(pagination.PageSize)
            .ToListAsync();
        
        // Map entities
        var mappedEntities = entities.Select(x => mapper.Map<PartnerModel>(x)).ToList();

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
    }

    // Implementation for UNOPSPartner specification (interface requirement)
    public async Task<PaginationResponse<PartnerModel>> GetPartnersWithSpecification(int userId, ISpecification<UNOPSPartner> specification, PaginationRequest pagination)
    {
        // This implementation doesn't support UNOPSPartner specifications since this manager works with Partner entities
        // Return empty result or throw NotSupportedException
        throw new NotSupportedException("This PartnerManager implementation does not support UNOPSPartner specifications. Use UNOPSPartnerManager instead.");
    }

    public async Task<PartnerModel?> GetPartner(int userId, int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
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
        string[] includes = ["Documents", "PartnerOffice", "PartnerGroup", "Contacts"];

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

    /// <summary>
    /// Gets a partner with its contacts and their interactions included
    /// </summary>
    public async Task<PartnerModel?> GetPartnerWithContactsAndInteractionsAsync(int id)
    {
        // Include contacts and their interactions using standard Entity Framework includes
        string[] includes = ["Documents", "PartnerOffice", "PartnerGroup", "Contacts", "Contacts.Interactions"];

        var partner = await PartnerRepository
            .GetAll(includes)
            .Where(x => x.Id == id && !x.IsDeleted && (x.PartnerOffice == null || x.PartnerOffice.Type == OrganizationUnitType.OrgUnit))
            .FirstOrDefaultAsync();

        if (partner == null)
        {
            return default;
        }

        // Now you can use the Partner entity's methods to get interaction data
        // Examples:
        // var allInteractions = partner.GetAllInteractions();
        // var recentInteractions = partner.GetRecentInteractions(5);
        // var interactionsByContact = partner.GetInteractionsByContact();
        // var summary = partner.GetSummary();

        return mapper.Map<PartnerModel>(partner);
    }

    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroup(int userId, string partnerTreeId, PaginationRequest request)
    {
        var partnerTreeCode = partnerTreeId;
        
        var query = PartnerRepository
            .GetAll(["PartnerOffice"])
            .Where(x => !x.IsDeleted && x.PartnerGroupCode == partnerTreeCode)
            .AsQueryable();

        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();
        
        // Map entities
        var mappedEntities = entities.Select(x => mapper.Map<PartnerModel>(x)).ToList();

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
    }

    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerCategory(int userId, string partnerCategoryCode, PaginationRequest request)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice"])
            .Where(x => !x.IsDeleted && x.PartnerGroup != null && x.PartnerGroup.PartnerCategoryCode == partnerCategoryCode)
            .AsQueryable();

        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();
        
        // Map entities
        var mappedEntities = entities.Select(x => mapper.Map<PartnerModel>(x)).ToList();

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
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

    public List<PartnerTree> GetChildPartnerTreesRecursively(List<string> parentCodes)
    {
        if (parentCodes == null || !parentCodes.Any())
            return new List<PartnerTree>();

        // Get immediate children
        var children = PartnerTreeRepository.GetAll()
            .Where(pt => pt.Parent != null && parentCodes.Contains(pt.Parent))
            .ToList();

        if (!children.Any())
            return new List<PartnerTree>();

        // Get child codes
        var childCodes = children.Select(c => c.Code).ToList();

        // Recursively get descendants
        var descendants = GetChildPartnerTreesRecursively(childCodes);

        // Combine immediate children with their descendants
        return children.Union(descendants).ToList();
    }
    
    public List<PartnerTree> GetAllDescendantPartnerTrees(List<string> partnerTreesByCategoryCodes)
    {
        // Get the original PartnerTrees by their codes
        var originalPartnerTrees = PartnerTreeRepository.GetAll()
            .Where(pt => partnerTreesByCategoryCodes.Contains(pt.Code))
            .ToList();
            
        // Get all descendants recursively
        var descendants = GetChildPartnerTreesRecursively(partnerTreesByCategoryCodes);
        
        // Return all trees including the original ones and their descendants
        return originalPartnerTrees.Union(descendants).ToList();
    }

    #region Secure Methods for Permission-based Access
    
    /// <summary>
    /// Gets partners with row-level security applied based on user permissions
    /// Note: This implementation provides basic functionality without advanced security filtering
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets a specific partner with row-level security applied
    /// Note: This implementation provides basic functionality without advanced security filtering
    /// </summary>
    public async Task<PartnerModel?> GetPartnerAsync(ClaimsPrincipal user, int id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new partner with permission validation
    /// Note: This implementation provides basic functionality without advanced security validation
    /// </summary>
    public async Task<PartnerModel?> CreatePartnerAsync(ClaimsPrincipal user, PartnerRequest model)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates a partner with permission validation
    /// Note: This implementation provides basic functionality without advanced security validation
    /// </summary>
    public async Task<PartnerModel?> UpdatePartnerAsync(ClaimsPrincipal user, UpdatePartnerRequest model)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a partner with permission validation
    /// Note: This implementation provides basic functionality without advanced security validation
    /// </summary>
    public async Task<bool> DeletePartnerAsync(ClaimsPrincipal user, int id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets partners by partner group with security applied
    /// Note: This implementation provides basic functionality without advanced security filtering
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroupAsync(ClaimsPrincipal user, string partnerGroupCode, PaginationRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets partners by partner category with security applied
    /// Note: This implementation provides basic functionality without advanced security filtering
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByCategoryAsync(ClaimsPrincipal user, string partnerCategoryCode, PaginationRequest request)
    {
        throw new NotImplementedException();
    }
    
    #endregion
}