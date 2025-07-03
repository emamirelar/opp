using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Specifications;
using System.Linq;
using UNOPS.PAO.UNOPSBusiness.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

public class UNOPSPartnerManager : BaseUNOPSManager, IPartnerManager
{
    private readonly IMapper _mapper;
    private readonly UNOPSAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UNOPSPartnerManager> _logger;

    private BaseRepository<UNOPSPartner> PartnerRepository;
    private BaseRepository<OrganizationHierarchy> OrganizationHierarchyRepository;
    private BaseRepository<UNOPSPartnerTree> PartnerTreeRepository;
    private PartnerTreeService PartnerTreeService;

    private CommonEntityRepository commonRepository;


    private GoogleCloudStorageService GoogleCloudStorageService;

    //private string[] includes = ["Currency", "Documents"];

    private async Task<PartnerModel> MapEntityToModelAsync(UNOPSPartner entity, IMapper mapper, ClaimsPrincipal user = null)
    {
        // Use AutoMapper with the updated configuration
        var result = mapper.Map<UNOPSPartner, PartnerModel>(entity);

        // Convert LogoUrl to signed URL if it exists and contains Google Cloud Storage path
        if (!string.IsNullOrEmpty(result.LogoUrl) && GoogleCloudStorageService != null)
        {
            result.LogoUrl = await GoogleCloudStorageService.GenerateSignedUrlFromStorageUrl(result.LogoUrl);
        }

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

        // Use the provided user or get current user context
        var userContext = user ?? GetCurrentUserOrSystemContext();
        return await MapEntityToModelWithPermissionsAsync(result, userContext); ;
    }

    /*private async Task<PartnerModel> MapEntityToModelWithPermissionsAsync(UNOPSPartner entity, IMapper mapper, ClaimsPrincipal? user = null)
    {
        var result = await MapEntityToModelAsync(entity, mapper);
        
        // Add permissions if user context is available
        if (user != null && _securityService != null)
        {
            var permissions = await _securityService.GetEntityPermissionsAsync(entity, user);
            result.Permissions = new EntityPermissionsModel
            {
                CanRead = ((dynamic)permissions).canRead,
                CanCreate = await _securityService.CanUserAccessEntityAsync(entity, user, "create"),
                CanUpdate = await _securityService.CanUserAccessEntityAsync(entity, user, "update"),
                CanDelete = await _securityService.CanUserAccessEntityAsync(entity, user, "delete")
            };
        }
        else
        {
            // Default permissions when no user context available
            result.Permissions = new EntityPermissionsModel
            {
                CanRead = true, // Assume readable if no security context
                CanCreate = false,
                CanUpdate = false, // Default to no write access
                CanDelete = false
            };
        }

        return result;
    }*/

    private PartnerModel MapEntityToModel(UNOPSPartner entity, IMapper mapper)
    {
        // Use AutoMapper with the updated configuration
        var result = mapper.Map<UNOPSPartner, PartnerModel>(entity);

        if (result.PartnerGroupCode != null && PartnerTreeService != null)
        {
            // Note: This is a synchronous version, so we can't await async calls
            // For full functionality, use MapEntityToModelAsync instead
            // This method is used in LINQ expressions where async is not supported
        }

        return result;
    }

    private UNOPSPartner MapModelToEntity(PartnerRequest model, UNOPSPartner entity)
    {
        _mapper.Map(model, entity);
        return entity;
    }

    private UNOPSPartner MapModelToEntity(PartnerRequest model)
    {
        return MapModelToEntity(model, new UNOPSPartner());
    }

    public UNOPSPartnerManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService, ILogger<UNOPSPartnerManager> logger, IPermissionService permissionService = null, IHttpContextAccessor httpContextAccessor = null)
        : base(mapper, context, configuration, null, "Partner", permissionService, httpContextAccessor)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
        _logger = logger;
       // _securityService = securityService;
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

        return await MapEntityToModelAsync(entity, _mapper, null);
    }

    public async Task<PaginationResponse<PartnerModel>> GetPartners(int userId, PaginationRequest request)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup", "Contacts"])
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
            var mapped = await MapEntityToModelAsync(entity, _mapper, null);
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
        
        // Map entities asynchronously with default permissions
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in entities)
        {
            var mapped = await MapEntityToModelAsync((UNOPSPartner)entity, _mapper, null);
            mappedEntities.Add(mapped);
        }

        return new PaginationResponse<PartnerModel>
        {
            TotalCount = totalCount,
            Records = mappedEntities
        };
    }

    public async Task<object> GetPartnersWithSpecificationAsync(ClaimsPrincipal user, ISpecification<Partner> specification, PaginationRequest pagination)
    {
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        var filteredQuery = query.ApplySpecification(specification);
        
        // OrgUnit filtering is now handled by the specification created by OrgUnitFilterService
        // No need for duplicate logic here
        
        // Apply access control filters (row and column filtering) BEFORE pagination
        var filteredData = await ApplyAccessControlFilters(filteredQuery, user, "read");
        
        // If filteredData is a list, we need to handle pagination manually
        // Note: ApplyAccessControlFilters returns List<Partner> but we need to handle it as Partner
        if (filteredData is IEnumerable<Partner> basePartnerList)
        {
            // Cast back to UNOPSPartner since we know all items in the query are UNOPSPartner
            var partnerArray = basePartnerList.Cast<UNOPSPartner>().ToArray();
            var totalCount = partnerArray.Length;
            var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
            var excludedRows = (pageIndex - 1) * pagination.PageSize;
            
            var pagedItems = partnerArray
                .Skip(excludedRows)
                .Take(pagination.PageSize)
                .ToArray();

            var results = new List<PartnerModel>();
            foreach (var item in pagedItems)
            {
                var mapped = await MapEntityToModelAsync(item, _mapper, user);
                results.Add(mapped);
            }

            return new PaginationResponse<PartnerModel>
            {
                Records = results,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pagination.PageSize
            };
        }

        // Fallback: if filteredData is not the expected type, return empty result
        return new PaginationResponse<PartnerModel>
        {
            Records = new List<PartnerModel>(),
            TotalCount = 0,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize
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

        return await MapEntityToModelAsync(item, _mapper, null);
    }

    /// <summary>
    /// Gets basic partner details without contacts and interactions - designed for AI prompts
    /// </summary>
    public async Task<PartnerModel?> GetBasicPartnerDetailsAsync(int id)
    {
        string[] includes = ["PartnerOffice", "PartnerGroup"];

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

        return await MapEntityToModelAsync(item, _mapper, null);
    }

    /// <summary>
    /// Gets a partner with its contacts and their interactions included
    /// </summary>
    public async Task<PartnerModel?> GetPartnerWithContactsAndInteractionsAsync(int id)
    {
        // Get partner with basic includes first
        string[] includes = ["Documents", "PartnerOffice", "PartnerGroup", "Contacts"];

        var partner = await PartnerRepository.GetByIdAsync(id, includes);

        if (partner == null)
        {
            return default;
        }

        // Manually load interactions for each contact through the InteractionContacts junction table
        if (partner.Contacts != null && partner.Contacts.Any())
        {
            var contactIds = partner.Contacts.Select(c => c.Id).ToList();
            
            // Get interactions through the junction table
            var interactionContacts = await _context.InteractionContacts
                .Where(ic => contactIds.Contains(ic.ContactId))
                .Include(ic => ic.Interaction)
                .ToListAsync();

            // Group interactions by contact
            var interactionsByContact = interactionContacts
                .GroupBy(ic => ic.ContactId)
                .ToDictionary(g => g.Key, g => g.Select(ic => ic.Interaction).ToList());

            // Assign interactions to each contact
            foreach (var contact in partner.Contacts)
            {
                if (interactionsByContact.TryGetValue(contact.Id, out var interactions))
                {
                    contact.Interactions = interactions;
                }
            }
        }

        // Load partner office if needed
        if (partner.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationHierarchyRepository.GetByIdAsync(partner.PartnerOfficeId.Value);
            if (partnerOffice != null)
            {
                partner.PartnerOffice = partnerOffice;
            }
        }

        // Now you can use the Partner entity's methods to get interaction data
        // Examples:
        // var allInteractions = partner.GetAllInteractions();
        // var recentInteractions = partner.GetRecentInteractions(5);
        // var interactionsByContact = partner.GetInteractionsByContact();
        // var summary = partner.GetSummary();

        return await MapEntityToModelAsync(partner, _mapper, null);
    }

    /// <summary>
    /// Gets a partner with its associated projects through the many-to-many relationship
    /// </summary>
    public async Task<PartnerModel?> GetPartnerWithProjectsAsync(int id)
    {
        // Include the projects through the many-to-many relationship
        string[] includes = ["Documents", "PartnerOffice", "PartnerGroup", "Projects"];

        var partner = await PartnerRepository.GetByIdAsync(id, includes);

        if (partner == null)
        {
            return default;
        }

        // Load partner office if needed
        if (partner.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationHierarchyRepository.GetByIdAsync(partner.PartnerOfficeId.Value);
            if (partnerOffice != null)
            {
                partner.PartnerOffice = partnerOffice;
            }
        }

        var result = await MapEntityToModelAsync(partner, _mapper, null);

        // Map the projects to ProjectSummaryModel
        if (partner.Projects != null && partner.Projects.Any())
        {
            result.Projects = partner.Projects.Select(project => new ProjectSummaryModel
            {
                Id = project.Id,
                ProjectNumber = project.ProjectNumber,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Stage = project.Stage,
                BudgetCheckingLevel = project.BudgetCheckingLevel,
                BudgetDuration = project.BudgetDuration,
                BudgetAmount = project.BudgetAmount,
                ExpenditureAmount = project.ExpenditureAmount
            }).ToList();
        }

        return result;
    }

    /// <summary>
    /// Gets partner risk profile with comprehensive details including projects - designed for risk analysis and AI prompts
    /// </summary>
    public async Task<PartnerModel?> GetPartnerRiskProfileAsync(int id)
    {
        // Include all relevant data for risk assessment: documents, office, group, contacts, interactions, and projects
        string[] includes = ["Documents", "PartnerOffice", "PartnerGroup", "Contacts", "Contacts.Interactions", "Projects"];

        var partner = await PartnerRepository.GetByIdAsync(id, includes);

        if (partner == null)
        {
            return default;
        }

        // Load partner office if needed
        if (partner.PartnerOfficeId.HasValue)
        {
            var partnerOffice = await OrganizationHierarchyRepository.GetByIdAsync(partner.PartnerOfficeId.Value);
            if (partnerOffice != null)
            {
                partner.PartnerOffice = partnerOffice;
            }
        }

        var result = await MapEntityToModelAsync(partner, _mapper, null);

        // Map the projects to ProjectSummaryModel
        if (partner.Projects != null && partner.Projects.Any())
        {
            result.Projects = partner.Projects.Select(project => new ProjectSummaryModel
            {
                Id = project.Id,
                ProjectNumber = project.ProjectNumber,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Stage = project.Stage,
                BudgetCheckingLevel = project.BudgetCheckingLevel,
                BudgetDuration = project.BudgetDuration,
                BudgetAmount = project.BudgetAmount,
                ExpenditureAmount = project.ExpenditureAmount
            }).ToList();
        }

        return result;
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
            
            // Map entities asynchronously with default permissions
            var mappedEntities = new List<PartnerModel>();
            foreach (var entity in entities)
            {
                var mapped = await MapEntityToModelAsync(entity, _mapper, null);
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
            .GetAll(["PartnerOffice", "PartnerGroup"])
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
        
        // Map entities asynchronously with default permissions
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in entities)
        {
            var mapped = await MapEntityToModelAsync(entity, _mapper, null);
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

    #region Secure Methods for Permission-based Access
    
    /// <summary>
    /// Gets all partners with row-level security applied
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        // RBAC interceptor handles security enforcement
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        var partners = query.Paginate(
            x => {
                var model = MapEntityToModel(x, _mapper);
                return model;
            },
            request
        );

        return partners;
    }

    /// <summary>
    /// Gets a specific partner with row-level security applied
    /// </summary>
    public async Task<PartnerModel?> GetPartnerAsync(ClaimsPrincipal user, int id)
    {
        var item = await PartnerRepository.GetByIdAsync(id, ["PartnerOffice", "PartnerGroup"]);
        if (item == null)
        {
            return null;
        }

        // Check if user has permission to access this specific entity
        // Create a single-item query and apply access control filters
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup"])
            .Where(x => x.Id == id && !x.IsDeleted)
            .AsQueryable();

        // Apply access control filters (row and column filtering)
        var filteredData = await ApplyAccessControlFilters(query, user, "read");
        
        // If filteredData is a list and contains our entity, user has access
        if (filteredData is IEnumerable<UNOPSPartner> partnerList)
        {
            var accessiblePartner = partnerList.FirstOrDefault();
            if (accessiblePartner != null)
            {
                return await MapEntityToModelAsync(accessiblePartner, _mapper, user);
            }
        }

        // User doesn't have access to this entity
        return null;
    }

    /// <summary>
    /// Creates a new partner with permission validation
    /// </summary>
    public async Task<PartnerModel?> CreatePartnerAsync(ClaimsPrincipal user, PartnerRequest model)
    {
        // RBAC interceptor handles security enforcement
        var entity = MapModelToEntity(model);
        await PartnerRepository.AddAsync(entity);

        var resultModel = MapEntityToModel(entity, _mapper);
        
        // Add permissions for frontend UI
        //resultModel.Permissions = await GetEntityPermissionsAsync(entity, user);

        return resultModel;
    }

    /// <summary>
    /// Updates a partner with permission validation
    /// </summary>
    public async Task<PartnerModel?> UpdatePartnerAsync(ClaimsPrincipal user, UpdatePartnerRequest model)
    {
        // RBAC interceptor handles security enforcement
        var entity = await PartnerRepository.GetByIdAsync(model.Id, ["PartnerOffice"]);
        if (entity == null)
        {
            return null;
        }

        // Update the entity
        _mapper.Map(model, entity);
        await PartnerRepository.UpdateAsync(entity);

        var resultModel = MapEntityToModel(entity, _mapper);
        
        // Add permissions for frontend UI
        //resultModel.Permissions = await GetEntityPermissionsAsync(entity, user);

        return resultModel;
    }

    /// <summary>
    /// Deletes a partner with permission validation
    /// </summary>
    public async Task<bool> DeletePartnerAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var entity = await PartnerRepository.GetByIdAsync(id, ["PartnerOffice"]);
        if (entity == null)
        {
            return false;
        }

        await PartnerRepository.Delete(entity);
        return true;
    }

    /// <summary>
    /// Gets partners by partner group with security applied
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroupAsync(ClaimsPrincipal user, string partnerGroupCode, PaginationRequest request)
    {
        // RBAC interceptor handles security enforcement
        // First get all partner trees with this group code
        var partnerTreesByGroup = PartnerTreeRepository.GetAll()
            .Where(pt => pt.Code == partnerGroupCode)
            .Distinct()
            .ToList();
            
        if (!partnerTreesByGroup.Any())
        {
            return new PaginationResponse<PartnerModel>
            {
                Records = [],
                TotalCount = 0
            };
        }
        
        var partnerTreesByGroupCodes = partnerTreesByGroup.Select(pt => pt.Code).ToList();
        var partnerTreesByGroupWithChildrenCodes = GetAllDescendantPartnerTrees(partnerTreesByGroupCodes).Select(pt => pt.Code).ToList();
        
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup"])
            .Where(x => !x.IsDeleted && partnerTreesByGroupWithChildrenCodes.Contains(x.PartnerGroupCode))
            .AsQueryable();

        var partners = query.Paginate(
            x => {
                var model = MapEntityToModel(x, _mapper);
                return model;
            },
            request
        );

        // Add permissions for frontend UI
        foreach (var partner in partners.Records)
        {
            /*partner.Permissions = await GetEntityPermissionsAsync(
                await PartnerRepository.GetByIdAsync(partner.Id, ["PartnerOffice"]), 
                user
            );
            */
        }

        return partners;
    }

    /// <summary>
    /// Gets partners by partner category with security applied
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByCategoryAsync(ClaimsPrincipal user, string partnerCategoryCode, PaginationRequest request)
    {
        var partnerTreesByCategory = PartnerTreeRepository.GetAll()
            .Where(pt => pt.PartnerCategoryCode != null
                ? pt.PartnerCategoryCode == partnerCategoryCode
                : pt.Code == partnerCategoryCode)
            .Distinct()
            .ToList();
            
        if (!partnerTreesByCategory.Any())
        {
            return new PaginationResponse<PartnerModel>
            {
                Records = [],
                TotalCount = 0
            };
        }
        
        var partnerTreesByCategoryCodes = partnerTreesByCategory.Select(pt => pt.Code).ToList();
        var partnerTreesByGroupInCategoryCode = GetAllDescendantPartnerTrees(partnerTreesByCategoryCodes).Select(pt => pt.Code).ToList();
        
        var query = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup"])
            .Where(x => !x.IsDeleted && partnerTreesByGroupInCategoryCode.Contains(x.PartnerGroupCode))
            .AsQueryable();

        // Apply access control filters (row and column filtering)
        var filteredData = await ApplyAccessControlFilters(query, user, "read");
        
        // If filteredData is a list, we need to handle pagination manually
        if (filteredData is IEnumerable<UNOPSPartner> partnerList)
        {
            var partnerArray = partnerList.ToArray();
            var totalCount = partnerArray.Length;
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var excludedRows = (pageIndex - 1) * request.PageSize;
            
            var pagedItems = partnerArray
                .Skip(excludedRows)
                .Take(request.PageSize)
                .ToArray();

            var results = new List<PartnerModel>();
            foreach (var item in pagedItems)
            {
                var mapped = await MapEntityToModelAsync(item, _mapper, user);
                results.Add(mapped);
            }

            return new PaginationResponse<PartnerModel>
            {
                Records = results,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = request.PageSize
            };
        }

        var partners = query.Paginate(
            x => {
                var model = MapEntityToModel(x, _mapper);
                return model;
            },
            request
        );

        return partners;
    }
    
    #endregion

    #region Interface Methods (Legacy - without ClaimsPrincipal)
    
    public async Task<PartnerModel?> GetPartnerAsync(int id)
    {
        string[] includes = ["Documents", "PartnerOffice", "PartnerGroup", "Contacts"];

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

        return await MapEntityToModelAsync(item, _mapper, null);
    }

    public async Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model)
    {
        var entity = await PartnerRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await PartnerRepository.UpdateAsync(entity);

        return await MapEntityToModelAsync(entity, _mapper, null);
    }

    public async Task DeletePartnerAsync(int userId, int id)
    {
        var entity = await PartnerRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await PartnerRepository.Delete(entity);
        }
    }

    public async Task<PartnerModel?> GetPartnerAsync(int userId, int id)
    {
        // Use the original implementation but fix to match the interface
        return await GetPartnerAsync(id);
    }

    public async Task<List<PartnerModel?>> GetPartnersForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user = null)
    {
        var partners = PartnerRepository
            .GetAll(["PartnerOffice", "PartnerGroup", "Contacts"])
            .AsQueryable()
            .Where(p => input.partnerIds.Contains(p.Id))
            .Cast<UNOPSPartner>()
            .ToList();

        // Batch permission lookup once for all partners
        var userPartnerPermissions = await GetEntityPermissionsAsync(user, "Partner");
        // Batch permission lookup once for all contacts
        //var userContactPermissions = await GetEntityPermissionsAsync(user, "Contact");

        var mappedPartners = new List<PartnerModel>();
        foreach (var partner in partners)
        {
            var model = await MapEntityToModelAsync(partner, _mapper);
            /*model.Permissions = new EntityPermissionsModel
            {
                CanRead = userPartnerPermissions.CanRead,
                CanCreate = userPartnerPermissions.CanCreate,
                CanUpdate = userPartnerPermissions.CanUpdate && await _securityService.CanUserAccessEntityAsync(partner, user, "update"),
                CanDelete = userPartnerPermissions.CanDelete && await _securityService.CanUserAccessEntityAsync(partner, user, "delete")
            };*/

            if(partner.First5ContactsByDate != null && partner.First5ContactsByDate.Count() > 0)
            {
                foreach (var contact in partner.First5ContactsByDate)
                {
                    // Map each contact to ContactModel
                    var contactModel = _mapper.Map<ContactModel>(contact);
                    // Add permissions for each contact
                    //Checking the permissions for contacts separately as the basemanager checks for the current entity only
                    contactModel.Permissions = new EntityPermissionsModel
                    {
                        CanRead = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "read"),
                        CanCreate = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "create"),
                        CanUpdate = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "update"),
                        CanDelete = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "delete")
                    };
                    model.First5ContactsByDate.RemoveAll(c => c.Id == contactModel.Id);
                    model.First5ContactsByDate.Add(contactModel);
                }
            }

            mappedPartners.Add(model);
        }


        return mappedPartners;
    }

    #endregion

    /// <summary>
    /// Implementation of abstract method from BaseUNOPSManager
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        return await GetPartnerAsync(user, entityId);
    }
}