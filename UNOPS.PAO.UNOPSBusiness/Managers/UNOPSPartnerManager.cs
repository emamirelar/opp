using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Specifications;
using System.Linq;
using UNOPS.PAO.UNOPSBusiness.Services;

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
using System.Security.Claims;

public class UNOPSPartnerManager : IPartnerManager
{
    private readonly IMapper _mapper;
    private readonly UNOPSAppDbContext _context;
    private readonly IConfiguration _configuration;
    private BaseRepository<UNOPSPartner> PartnerRepository;
    private BaseRepository<OrganizationHierarchy> OrganizationHierarchyRepository;
    private BaseRepository<UNOPSPartnerTree> PartnerTreeRepository;
    private PartnerTreeService PartnerTreeService;

    private CommonEntityRepository commonRepository;


    private GoogleCloudStorageService GoogleCloudStorageService;

    //private string[] includes = ["Currency", "Documents"];

    private async Task<PartnerModel> MapEntityToModelAsync(UNOPSPartner entity, IMapper mapper)
    {
        // Use AutoMapper with the updated configuration
        var result = mapper.Map<UNOPSPartner, PartnerModel>(entity);

        if (result.PartnerGroupCode != null && PartnerTreeService != null)
        {
            var partnerTreeGroup = await PartnerTreeService.GetPartnerTreeByCodeAsync(result.PartnerGroupCode);
            var partnerTreeCategory = await PartnerTreeService.GetPartnerCategoryByPartnerGroupCodeAsync(result.PartnerGroupCode);
            if (partnerTreeGroup != null) {
                result.PartnerGroupName = partnerTreeGroup.Name;
                result.PartnerGroupCode = partnerTreeGroup.Code;
                result.PartnerGroupId = partnerTreeGroup.Id;
            }

            if (partnerTreeCategory != null) {
                result.PartnerCategoryCode = partnerTreeCategory.Code;
                result.PartnerCategoryName = partnerTreeCategory.Name;
                result.PartnerCategoryId = partnerTreeCategory.Id;
            }
        }

        return result;
    }

    private UNOPSPartner MapModelToEntity(PartnerRequest model, UNOPSPartner entity)
    {
        _mapper.Map(model, entity);
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

    public UNOPSPartnerManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
        PartnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        PartnerTreeRepository = new BaseRepository<UNOPSPartnerTree>(context, configuration);
        OrganizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration);
        
        PartnerTreeService = partnerTreeService;
        
        GoogleCloudStorageService = new GoogleCloudStorageService(configuration);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<PartnerModel> CreatePartnerAsync(PartnerRequest model)
    {
        var entity = MapModelToEntity(model);

        await PartnerRepository.AddAsync(entity);

        return _mapper.Map<PartnerModel>(entity);
    }

    public async Task<PaginationResponse<PartnerModel>> GetPartners(int userId, PaginationRequest request)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerCategory", "Contacts"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Get total count
        var totalCount = query.Count();
        
        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToList();
        
        // Map entities asynchronously
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in entities)
        {
            var mapped = await MapEntityToModelAsync(entity, _mapper);
            mappedEntities.Add(mapped);
        }

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
    }

    public async Task<PaginationResponse<PartnerModel>> GetPartnersWithSpecification(int userId, ISpecification<Partner> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = PartnerRepository.GetAll(["Contacts"]).AsQueryable();
        var filteredQuery = query.ApplySpecification(specification);
        
        // Get total count
        var totalCount = filteredQuery.Count();
        
        // Apply pagination
        var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        var excludedRows = (pageIndex - 1) * pagination.PageSize;
        
        if (pagination.OrderBy != null)
        {
            filteredQuery = filteredQuery.OrderByColumnName(pagination.OrderBy, pagination.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = filteredQuery
            .Skip(excludedRows)
            .Take(pagination.PageSize)
            .ToList();
        
        // Map entities asynchronously
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in entities)
        {
            var mapped = await MapEntityToModelAsync((UNOPSPartner)entity, _mapper);
            mappedEntities.Add(mapped);
        }

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
    }

    public async Task<PartnerModel?> GetPartner(int userId, int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);
        if (item == null)
        {
            return default;
        }
        
        // Load partner office if needed
        if (item.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationHierarchyRepository.GetByIdAsync(item.PartnerOfficeId.Value);
            if (partnerOffice != null)
            {
                item.PartnerOffice = partnerOffice;
            }
        }

        return await MapEntityToModelAsync(item, _mapper);
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
            .Select(x => MapEntityToExternalModel(x, _mapper));
    }

    public async Task<ExternalPartnerModel?> GetPostedPartner(int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner {id} does not exist.");
        }

        return MapEntityToExternalModel(item, _mapper);
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

        return await MapEntityToModelAsync(entity, _mapper);
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

        return _mapper.Map<PartnerModel>(entity);
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
        string[] includes = ["Documents", "PartnerOffice", "PartnerCategory", "Contacts"];

        var item = await PartnerRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        // Load partner office if needed
        if (item.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationHierarchyRepository.GetByIdAsync(item.PartnerOfficeId.Value);
            if (partnerOffice != null)
            {
                item.PartnerOffice = partnerOffice;
            }
        }

        return await MapEntityToModelAsync(item, _mapper);
    }
    
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroup(int userId, string partnerGroupCode, PaginationRequest request)
    {
        // Add logging
        Console.WriteLine($"GetPartnersByPartnerGroup called with partnerGroupCode: {partnerGroupCode}");
        var partnerTree = PartnerTreeRepository.GetAll()
            .FirstOrDefault(pt => pt.PartnerGroupCode == partnerGroupCode);
        
        try
        {
            // First get the partner tree by ID
            if (partnerTree == null)
            {
                Console.WriteLine($"No partner tree found with PartnerGroupCode: {partnerGroupCode}");
                // If no partner tree found, return empty result
                return new PaginationResponse<PartnerModel>
                {
                    Records = new List<PartnerModel>(),
                    TotalCount = 0
                };
            }
            
            // Get the code from the partner tree
            var code = partnerTree.Code;
            Console.WriteLine($"Found partner tree with Code: {code}");
            
            // Get all partners with the matching code
            var query = PartnerRepository
                .GetAll()
                .Where(x => !x.IsDeleted && x.PartnerGroupCode == code)
                .AsQueryable();

            // Get total count
            var totalCount = query.Count();
            
            // Apply pagination
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var excludedRows = (pageIndex - 1) * request.PageSize;
            
            if (request.OrderBy != null)
            {
                query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
            }
            
            // Get the entities for this page
            var entities = query
                .Skip(excludedRows)
                .Take(request.PageSize)
                .ToList();
            
            // Map entities asynchronously
            var mappedEntities = new List<PartnerModel>();
            foreach (var entity in entities)
            {
                var mapped = await MapEntityToModelAsync(entity, _mapper);
                mappedEntities.Add(mapped);
            }
            
            var result = new PaginationResponse<PartnerModel>
            {
                TotalCount = totalCount,
                Records = mappedEntities
            };
            
            Console.WriteLine($"Found {result.TotalCount} partners matching PartnerGroupCode: {code}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPartnersByPartnerGroup: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // If no partner tree found, return empty result
            return new PaginationResponse<PartnerModel>
            {
                Records = [],
                TotalCount = 0
            };
        }
    }
    
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerCategory(int userId, string partnerCategoryCode, PaginationRequest request)
    {
        // First get all partner trees with this category code
        // By default take the Partner Tree CODE
        var partnerTreesByCategory = PartnerTreeRepository.GetAll()
            .Where(pt => pt.PartnerCategoryCode != null
                ? pt.PartnerCategoryCode == partnerCategoryCode
                : pt.Code == partnerCategoryCode)
            .Distinct()
            .ToList();
            
        if (!partnerTreesByCategory.Any())
        {
            // If no partner trees found, return empty result
            return new PaginationResponse<PartnerModel>
            {
                Records = [],
                TotalCount = 0
            };
        }
        
        // Get all the codes from the partner trees
        var partnerTreesByCategoryCodes = partnerTreesByCategory.Select(pt => pt.Code).ToList();
        
        var partnerTreesByGroupInCategoryCode = GetAllDescendantPartnerTrees(partnerTreesByCategoryCodes).Select(pt => pt.Code).ToList();
        
        // Get all partners with the matching codes
        var query = PartnerRepository
            .GetAll()
            .Where(x => !x.IsDeleted && partnerTreesByGroupInCategoryCode.Contains(x.PartnerGroupCode))
            .AsQueryable();

        // Get total count
        var totalCount = query.Count();
        
        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the entities for this page
        var entities = query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToList();
        
        // Map entities asynchronously
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in entities)
        {
            var mapped = await MapEntityToModelAsync(entity, _mapper);
            mappedEntities.Add(mapped);
        }

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

        try
        {
            // Upload the file to Google Cloud Storage
            var fileName = $"partners/{partnerId}/logo_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var publicUrl = await GoogleCloudStorageService.UploadFileAsync(file, fileName);
            
            // Update the entity with the logo URL
            entity.LogoUrl = publicUrl;
            await PartnerRepository.UpdateAsync(entity);
            
            return publicUrl;
        }
        catch (Exception ex)
        {
            // Log the error and return null
            Console.WriteLine($"Error uploading logo: {ex.Message}");
            return null;
        }
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