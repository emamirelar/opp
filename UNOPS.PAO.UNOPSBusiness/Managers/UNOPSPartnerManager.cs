using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using System.Linq;
using System.Reflection;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Specifications;
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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using UNOPS.PAO.UNOPSBusiness.Extensions;

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

        if (result.PartnerGroupId != null && PartnerTreeService != null)
        {
            var partnerTreeGroup = await PartnerTreeService.GetPartnerTreeByIdAsync(result.PartnerGroupId.Value);
            var partnerTreeCategory = await PartnerTreeService.GetPartnerCategoryByPartnerGroupCodeAsync(result.PartnerGroupCode);
            if (partnerTreeGroup != null) {
                result.PartnerGroupName = partnerTreeGroup.Name;
                result.PartnerGroupId = partnerTreeGroup.Id;
            }

            if (partnerTreeCategory != null) {
                result.PartnerCategoryCode = partnerTreeCategory.Code;
                result.PartnerCategoryName = partnerTreeCategory.Name;
                result.PartnerCategoryId = partnerTreeCategory.Id;
            }
        }

        // Populate PartnerFocalPointUserName if PartnerFocalPointUserId exists
        if (result.PartnerFocalPointUserId.HasValue && result.PartnerFocalPointUserId.Value > 0)
        {
            var focalPointUser = await _context.PAOUsers
                .Where(u => u.Id == result.PartnerFocalPointUserId.Value)
                .FirstOrDefaultAsync();
            if (focalPointUser != null)
            {
                result.PartnerFocalPointUserName = focalPointUser.Email;
            }
        }

        // Use the provided user or get current user context
        var userContext = user ?? GetCurrentUserOrSystemContext();
        return await MapEntityToModelWithPermissionsAsync(result, userContext);
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

        if (result.PartnerGroupId != null && PartnerTreeService != null)
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

    public UNOPSPartnerManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService, ILogger<UNOPSPartnerManager> logger, IPermissionService permissionService = null, IHttpContextAccessor httpContextAccessor = null, IServiceProvider serviceProvider = null)
        : base(mapper, context, configuration, null, "Partner", permissionService, httpContextAccessor)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
        _logger = logger;
       // _securityService = securityService;
        PartnerRepository = new BaseRepository<UNOPSPartner>(context, configuration, serviceProvider);
        PartnerTreeRepository = new BaseRepository<UNOPSPartnerTree>(context, configuration, serviceProvider);
        OrganizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration, serviceProvider);
        
        PartnerTreeService = partnerTreeService;
        
        GoogleCloudStorageService = new GoogleCloudStorageService(configuration);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<PartnerModel> CreatePartnerAsync(PartnerRequest model)
    {
        // Ensure partner is created in Draft status
        if (model != null) {
            model.Status = EntityStatus.Draft.ToString();
        }

        var entity = MapModelToEntity(model);

        // Save the partner first to get its ID
        await PartnerRepository.AddAsync(entity);
        await PartnerRepository.UpdateAsync(entity);

        // Handle organization unit hierarchy IDs if specified - AFTER saving the partner
        if (model.OrganizationHierarchyIds != null && model.OrganizationHierarchyIds.Any())
        {
            var relationshipsToAdd = new List<OrganizationUnitRelationship>();
            
            foreach (var orgUnitId in model.OrganizationHierarchyIds)
            {
                var orgUnit = await OrganizationHierarchyRepository.GetByIdAsync(orgUnitId);
                if (orgUnit == null || orgUnit.Type != OrganizationUnitType.OrgUnit)
                {
                    throw new BusinessException($"Organization unit with ID {orgUnitId} must be of type OrgUnit");
                }
                
                // Create the organization unit relationship with the actual partner ID
                var newRelationship = new OrganizationUnitRelationship
                {
                    OrganizationHierarchyId = orgUnit.Id,
                    EntityId = entity.Id, // Now entity.Id has the actual saved ID
                    EntityType = nameof(Partner),
                    Name = $"Partner-{entity.Id}-{orgUnit.Code}",
                    Status = EntityStatus.Active
                };
                relationshipsToAdd.Add(newRelationship);
            }
            
            if (relationshipsToAdd.Any())
            {
                await _context.OrganizationUnitRelationships.AddRangeAsync(relationshipsToAdd);
                await _context.SaveChangesAsync();
                
                _logger?.LogInformation("Added {Count} organization unit relationships for partner {PartnerId}: [{Ids}]", 
                    relationshipsToAdd.Count, entity.Id, string.Join(", ", relationshipsToAdd.Select(r => r.OrganizationHierarchyId)));
            }
        }

        return await MapEntityToModelAsync(entity, _mapper, null);
    }

    public async Task<PaginationResponse<PartnerModel>> GetPartners(int userId, PaginationRequest request)
    {
        var query = PartnerRepository
                            .GetAll(["PartnerGroup", "Contacts"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

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

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Partner>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSPartner>();
        
        // Apply org unit filtering if the specification supports it
        filteredQuery = ApplyOrgUnitFilterIfSupported(filteredQuery, specification);
        
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
            .GetAll(["PartnerGroup", "Contacts", "LiaisonOffice"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Partner>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSPartner>();
        
        // Apply org unit filtering if the specification supports it
        filteredQuery = ApplyOrgUnitFilterIfSupported(filteredQuery, specification);
        
        // OrgUnit filtering is now handled by the specification and our manual join method
        
        // Apply access control filters (row and column filtering) BEFORE pagination
        // Cast the query to UNOPSPartner query for access control to maintain type consistency
        var unosPartnerQuery = filteredQuery.Cast<UNOPSPartner>();
        var filteredData = await ApplyAccessControlFilters(unosPartnerQuery, user, "read");
        
        // If filteredData is a list, we need to handle pagination manually
        if (filteredData is IEnumerable<UNOPSPartner> partnerList)
        {
            var partnerArray = partnerList.ToArray();
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
        var item = await PartnerRepository.GetByIdAsync(id, ["PartnerGroup"]);
        if (item == null)
        {
            return default;
        }

        // Load organization unit relationships for single partner
        await item.LoadOrganizationUnitRelationshipsAsync(_context);

        return await MapEntityToModelAsync(item, _mapper, null);
    }

    /// <summary>
    /// Gets basic partner details without contacts and interactions - designed for AI prompts
    /// </summary>
    public async Task<PartnerModel?> GetBasicPartnerDetailsAsync(int id)
    {
        string[] includes = ["PartnerGroup"];

        var item = await PartnerRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        // Load organization unit relationships for single partner
        await item.LoadOrganizationUnitRelationshipsAsync(_context);

        return await MapEntityToModelAsync(item, _mapper, null);
    }

    /// <summary>
    /// Gets a partner with its contacts and their interactions included
    /// </summary>
    public async Task<PartnerModel?> GetPartnerWithContactsAndInteractionsAsync(int id)
    {
        // Get partner with basic includes first
        string[] includes = ["Documents", "PartnerGroup", "Contacts"];

        var partner = await PartnerRepository.GetByIdAsync(id, includes);

        if (partner == null)
        {
            return default;
        }

        // Load organization unit relationships for single partner
        await partner.LoadOrganizationUnitRelationshipsAsync(_context);

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

        // OrganizationUnitRelationships are now loaded via includes

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
        string[] includes = ["Documents", "PartnerGroup", "Projects"];

        var partner = await PartnerRepository.GetByIdAsync(id, includes);

        if (partner == null)
        {
            return default;
        }

        // Load organization unit relationships for single partner
        await partner.LoadOrganizationUnitRelationshipsAsync(_context);

        // OrganizationUnitRelationships are now loaded via includes

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
        // Include all relevant data for risk assessment: documents, organization units, group, contacts, interactions, and projects
        string[] includes = ["Documents", "PartnerGroup", "Contacts", "Contacts.Interactions", "Projects"];

        var partner = await PartnerRepository.GetByIdAsync(id, includes);

        if (partner == null)
        {
            return default;
        }

        // Load organization unit relationships for single partner
        await partner.LoadOrganizationUnitRelationshipsAsync(_context);

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
    
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroup(int userId, int partnerGroupId, PaginationRequest request)
    {
        // Add logging
        Console.WriteLine($"GetPartnersByPartnerGroup called with partnerGroupId: {partnerGroupId}");
        var partnerTree = PartnerTreeRepository.GetAll()
            .FirstOrDefault(pt => pt.Id == partnerGroupId);
        
        try
        {
            // First get the partner tree by ID
            if (partnerTree == null)
            {
                Console.WriteLine($"No partner tree found with PartnerGroupId: {partnerGroupId}");
                // If no partner tree found, return empty result
                return new PaginationResponse<PartnerModel>
                {
                    Records = new List<PartnerModel>(),
                    TotalCount = 0
                };
            }
            
            // Get the id from the partner tree
            var id = partnerTree.Id;
            Console.WriteLine($"Found partner tree with Id: {id}");
            
            // Get all partners with the matching id
            var query = PartnerRepository
                .GetAll()
                .Where(x => !x.IsDeleted && x.PartnerGroupId == id)
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
            
            Console.WriteLine($"Found {result.TotalCount} partners matching PartnerGroupId: {id}");
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
        
        var partnerTreesByGroupInCategoryIds = GetAllDescendantPartnerTrees(partnerTreesByCategoryCodes).Select(pt => pt.Id).ToList();
        
        // Get all partners with the matching ids
        var query = PartnerRepository
            .GetAll(["PartnerGroup"])
            .Where(x => !x.IsDeleted && x.PartnerGroupId.HasValue && partnerTreesByGroupInCategoryIds.Contains(x.PartnerGroupId.Value))
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

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
            .GetAll(["PartnerGroup"])
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

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
        var item = await PartnerRepository.GetByIdAsync(id, ["PartnerGroup"]);
        if (item == null)
        {
            return null;
        }

        // Check if user has permission to access this specific entity
        // Create a single-item query and apply access control filters
        var query = PartnerRepository
            .GetAll(["PartnerGroup"])
            .Where(x => x.Id == id && !x.IsDeleted)
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

        // Apply access control filters (row and column filtering)
        var filteredData = await ApplyAccessControlFilters(query, user, "read");

        // If filteredData is a list and contains our entity, user has access
        if (filteredData is IEnumerable<UNOPSPartner> partnerList)
        {
            var accessiblePartner = partnerList.FirstOrDefault();
                    if (accessiblePartner != null)
        {
            // First map to model using AutoMapper
            var model = await MapEntityToModelAsync(accessiblePartner, _mapper, user);
            // Then add permissions using the entity (not model) for RBAC
            return await MapEntityToModelWithPermissionsAsync(model, user, accessiblePartner);
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
        
        // Validate minimum required fields (defensive check)
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new BusinessException("Partner Name is required for creation");
        }
        
        // Validate ErpDimValue uniqueness if provided
        if (model.ErpDimValue.HasValue)
        {
            var existingPartner = await _context.Partners
                .Where(p => p.ErpDimValue == model.ErpDimValue.Value && !p.IsDeleted)
                .FirstOrDefaultAsync();
            
            if (existingPartner != null)
            {
                throw new BusinessException($"A partner with ERP Dimension Value '{model.ErpDimValue.Value}' already exists. ERP Dimension Values must be unique.");
            }
        }
        
        // Ensure partner is created in Draft status
        model.Status = "Draft";
        
        var entity = MapModelToEntity(model);

        // Save the partner first to get its ID
        await PartnerRepository.AddAsync(entity);
        await PartnerRepository.UpdateAsync(entity);

        // Handle organization unit hierarchy IDs if specified - AFTER saving the partner
        if (model.OrganizationHierarchyIds != null && model.OrganizationHierarchyIds.Any())
        {
            var relationshipsToAdd = new List<OrganizationUnitRelationship>();
            
            foreach (var orgUnitId in model.OrganizationHierarchyIds)
            {
                var orgUnit = await OrganizationHierarchyRepository.GetByIdAsync(orgUnitId);
                if (orgUnit == null || orgUnit.Type != OrganizationUnitType.OrgUnit)
                {
                    throw new BusinessException($"Organization unit with ID {orgUnitId} must be of type OrgUnit");
                }
                
                // Create the organization unit relationship with the actual partner ID
                var newRelationship = new OrganizationUnitRelationship
                {
                    OrganizationHierarchyId = orgUnit.Id,
                    EntityId = entity.Id, // Now entity.Id has the actual saved ID
                    EntityType = nameof(Partner),
                    Name = $"Partner-{entity.Id}-{orgUnit.Code}",
                    Status = EntityStatus.Active
                };
                relationshipsToAdd.Add(newRelationship);
            }
            
            if (relationshipsToAdd.Any())
            {
                await _context.OrganizationUnitRelationships.AddRangeAsync(relationshipsToAdd);
                await _context.SaveChangesAsync();
                
                _logger?.LogInformation("Added {Count} organization unit relationships for partner {PartnerId}: [{Ids}]", 
                    relationshipsToAdd.Count, entity.Id, string.Join(", ", relationshipsToAdd.Select(r => r.OrganizationHierarchyId)));
            }
        }

        // First map to model using AutoMapper
        var resultModel = await MapEntityToModelAsync(entity, _mapper, user);
        // Then add permissions using the entity (not model) for RBAC
        resultModel = await MapEntityToModelWithPermissionsAsync(resultModel, user, entity);
        
        // Add permissions for frontend UI
        //resultModel.Permissions = await GetEntityPermissionsAsync(entity, user);

        return resultModel;
    }

    /// <summary>
    /// Efficiently updates organization unit relationships by only adding/removing what's changed
    /// </summary>
    private async Task UpdateOrganizationUnitRelationshipsDifferentialAsync(int partnerId, IEnumerable<int> newOrgUnitIds)
    {
        // Get current relationships from database
        var currentRelationships = await _context.OrganizationUnitRelationships
            .Where(r => r.EntityId == partnerId && r.EntityType == "Partner")
            .ToListAsync();

        var newOrgUnitIdsSet = new HashSet<int>(newOrgUnitIds ?? Enumerable.Empty<int>());
        var currentOrgUnitIds = new HashSet<int>(currentRelationships.Select(r => r.OrganizationHierarchyId));

        // Find relationships to remove (exist in current but not in new)
        var idsToRemove = currentOrgUnitIds.Except(newOrgUnitIdsSet).ToList();
        
        // Find relationships to add (exist in new but not in current)
        var idsToAdd = newOrgUnitIdsSet.Except(currentOrgUnitIds).ToList();

        // Remove relationships that are no longer needed
        if (idsToRemove.Any())
        {
            await _context.OrganizationUnitRelationships
                .Where(r => r.EntityId == partnerId && r.EntityType == "Partner" && idsToRemove.Contains(r.OrganizationHierarchyId))
                .ExecuteDeleteAsync();
            
            _logger?.LogInformation("Removed {Count} organization unit relationships for partner {PartnerId}: [{Ids}]", 
                idsToRemove.Count, partnerId, string.Join(", ", idsToRemove));
        }

        // Add new relationships
        if (idsToAdd.Any())
        {
            var relationshipsToAdd = new List<OrganizationUnitRelationship>();
            
            foreach (var orgUnitId in idsToAdd)
            {
                var orgUnit = await OrganizationHierarchyRepository.GetByIdAsync(orgUnitId);
                if (orgUnit != null && orgUnit.Type == OrganizationUnitType.OrgUnit)
                {
                    var newRelationship = new OrganizationUnitRelationship
                    {
                        OrganizationHierarchyId = orgUnit.Id,
                        EntityId = partnerId,
                        EntityType = nameof(Partner),
                        Name = $"Partner-{partnerId}-{orgUnit.Code}",
                        Status = EntityStatus.Active
                    };
                    relationshipsToAdd.Add(newRelationship);
                }
                else
                {
                    _logger?.LogWarning("Skipping invalid organization unit with ID {OrgUnitId} for partner {PartnerId}", orgUnitId, partnerId);
                }
            }
            
            if (relationshipsToAdd.Any())
            {
                await _context.OrganizationUnitRelationships.AddRangeAsync(relationshipsToAdd);
                await _context.SaveChangesAsync();
                
                _logger?.LogInformation("Added {Count} organization unit relationships for partner {PartnerId}: [{Ids}]", 
                    relationshipsToAdd.Count, partnerId, string.Join(", ", idsToAdd));
            }
        }

        // Log if no changes were needed
        if (!idsToRemove.Any() && !idsToAdd.Any())
        {
            _logger?.LogInformation("No organization unit relationship changes needed for partner {PartnerId}", partnerId);
        }
    }

    /// <summary>
    /// Updates a partner with permission validation
    /// </summary>
    public async Task<PartnerModel?> UpdatePartnerAsync(ClaimsPrincipal user, UpdatePartnerRequest model)
    {
        // RBAC interceptor handles security enforcement
        var entity = await PartnerRepository.GetByIdAsync(model.Id);
        if (entity == null)
        {
            return null;
        }

        // Validate ErpDimValue uniqueness if provided and different from current value
        if (model.ErpDimValue.HasValue && model.ErpDimValue.Value != entity.ErpDimValue)
        {
            var existingPartner = await _context.Partners
                .Where(p => p.ErpDimValue == model.ErpDimValue.Value && !p.IsDeleted && p.Id != model.Id)
                .FirstOrDefaultAsync();
            
            if (existingPartner != null)
            {
                throw new BusinessException($"A partner with ERP Dimension Value '{model.ErpDimValue.Value}' already exists. ERP Dimension Values must be unique.");
            }
        }

        // Handle organization unit hierarchy ID updates using differential approach
        if (model.OrganizationHierarchyIds != null)
        {
            await UpdateOrganizationUnitRelationshipsDifferentialAsync(entity.Id, model.OrganizationHierarchyIds);
        }
        
        // PatchNonNullProperties now automatically excludes navigation properties like OrganizationUnitRelationships
        // PatchNonNullProperties now automatically handles string-to-enum conversion
        PatchNonNullProperties(model, entity);
        
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
        var entity = await PartnerRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        // Load organization unit relationships for single partner
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);

        await PartnerRepository.Delete(entity);
        return true;
    }

    /// <summary>
    /// Gets partners by partner group with security applied
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroupAsync(ClaimsPrincipal user, int partnerGroupId, PaginationRequest request)
    {
        // RBAC interceptor handles security enforcement
        // First get all partner trees with this group id
        var partnerTreesByGroup = PartnerTreeRepository.GetAll()
            .Where(pt => pt.Id == partnerGroupId)
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
        var partnerTreesByGroupWithChildrenIds = GetAllDescendantPartnerTrees(partnerTreesByGroupCodes).Select(pt => pt.Id).ToList();
        
        var query = PartnerRepository
            .GetAll(["PartnerGroup"])
            .Where(x => !x.IsDeleted && x.PartnerGroupId.HasValue && partnerTreesByGroupWithChildrenIds.Contains(x.PartnerGroupId.Value))
            .AsQueryable();
        
        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

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
                await PartnerRepository.GetByIdAsync(partner.Id), 
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
        var partnerTreesByGroupInCategoryIds = GetAllDescendantPartnerTrees(partnerTreesByCategoryCodes).Select(pt => pt.Id).ToList();
        
        var query = PartnerRepository
            .GetAll(["PartnerGroup"])
            .Where(x => !x.IsDeleted && x.PartnerGroupId.HasValue && partnerTreesByGroupInCategoryIds.Contains(x.PartnerGroupId.Value))
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(_context);

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
        string[] includes = ["Documents", "PartnerGroup", "Contacts"];

        var item = await PartnerRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        // Load organization unit relationships
        await item.LoadOrganizationUnitRelationshipsAsync(_context);

        return await MapEntityToModelAsync(item, _mapper, null);
    }

    public async Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model)
    {
        var entity = await PartnerRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner {model.Id} does not exist.");
        }

        // Validate ErpDimValue uniqueness if provided and different from current value
        if (model.ErpDimValue.HasValue && model.ErpDimValue.Value != entity.ErpDimValue)
        {
            var existingPartner = await _context.Partners
                .Where(p => p.ErpDimValue == model.ErpDimValue.Value && !p.IsDeleted && p.Id != model.Id)
                .FirstOrDefaultAsync();
            
            if (existingPartner != null)
            {
                throw new BusinessException($"A partner with ERP Dimension Value '{model.ErpDimValue.Value}' already exists. ERP Dimension Values must be unique.");
            }
        }

        // Handle organization unit hierarchy ID updates using differential approach
        if (model.OrganizationHierarchyIds != null)
        {
            await UpdateOrganizationUnitRelationshipsDifferentialAsync(entity.Id, model.OrganizationHierarchyIds);
        }

        // PatchNonNullProperties now automatically excludes navigation properties like OrganizationUnitRelationships
        // PatchNonNullProperties now automatically handles string-to-enum conversion
        PatchNonNullProperties(model, entity);

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
        var partners = await _context.Partners
            .Where(p => input.partnerIds.Contains(p.Id))
            .Include(p => p.PartnerGroup)
            .ToListAsync();

        // Load organization unit relationships manually
        await partners.LoadOrganizationUnitRelationshipsAsync(_context);

        // Get all partner IDs to load interactions and contacts
        var allPartnerIds = partners.Select(p => p.Id).ToList();

        // Get all contacts for these partners
        var allContacts = await _context.Contacts
            .Where(c => allPartnerIds.Contains(c.PartnerId))
            .Cast<UNOPSContact>()
            .ToListAsync();

        // Group contacts by partner ID for efficient lookup
        var contactsByPartner = allContacts
            .GroupBy(c => c.PartnerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Get interactions through the InteractionPartners junction table with full interaction entities for permission checking
        var interactionPartners = await _context.InteractionPartners
            .Where(ip => allPartnerIds.Contains(ip.PartnerId))
            .Include(ip => ip.Interaction)
            .Select(ip => new
            {
                ip.PartnerId,
                Interaction = ip.Interaction
            })
            .ToListAsync();

        // Group interactions by partner ID for efficient lookup
        var interactionsByPartner = interactionPartners
            .GroupBy(ip => ip.PartnerId)
            .ToDictionary(g => g.Key, g => g.Select(ip => ip.Interaction).ToList());

        var mappedPartners = new List<PartnerModel>();
        foreach (var partner in partners)
        {
            var model = await MapEntityToModelAsync(partner, _mapper);

            // Add interactions directly to the partner with only Id, Type, Description, and Permissions
            if (interactionsByPartner.TryGetValue(partner.Id, out var partnerInteractions))
            {
                var interactionModels = new List<InteractionModel>();
                foreach (var interaction in partnerInteractions)
                {
                    var interactionModel = new InteractionModel
                    {
                        Id = interaction.Id,
                        Type = interaction.Type,
                        Description = interaction.Description,
                        Date = interaction.Date,
                        Permissions = new EntityPermissionsModel
                        {
                            CanRead = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "read"),
                            CanCreate = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "create"),
                            CanUpdate = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "update"),
                            CanDelete = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "delete")
                        }
                    };
                    interactionModels.Add(interactionModel);
                }
                model.Interactions = interactionModels;
            }

            // Add all contacts for this partner (not just first 5)
            if (contactsByPartner.TryGetValue(partner.Id, out var partnerContacts))
            {
                var contactModels = new List<ContactModel>();
                foreach (var contact in partnerContacts)
                {
                    // Map each contact to ContactModel
                    var contactModel = _mapper.Map<ContactModel>(contact);
                    
                    // Add permissions for each contact using direct permission service calls
                    contactModel.Permissions = new EntityPermissionsModel
                    {
                        CanRead = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "read"),
                        CanCreate = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "create"),
                        CanUpdate = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "update"),
                        CanDelete = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "delete")
                    };
                    
                    contactModels.Add(contactModel);
                }
                
                model.Contacts = contactModels;
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

    /// <summary>
    /// Gets basic partner data by ID without nested entities
    /// </summary>
    public override async Task<object> GetBasicEntityDataAsync(int id)
    {
        var partner = await _context.Partners
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (partner != null)
        {
            return _mapper.Map<UNOPSPartner, PartnerModel>(partner);
        }
        return null;
    }

    /// <summary>
    /// Gets multiple partners by their IDs for search results
    /// </summary>
    public override async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        if (ids == null || ids.Length == 0)
            return new List<object>();

        _logger?.LogInformation("UNOPSPartnerManager.GetByIdsAsync called with IDs: [{Ids}]", string.Join(", ", ids));

        var partners = PartnerRepository
            .GetAll(["PartnerGroup"])
            .Where(p => ids.Contains(p.Id))
            .ToList();

        await partners.LoadOrganizationUnitRelationshipsAsync(_context);

        _logger?.LogInformation("Found {Count} partners from database before RBAC filtering", partners.Count);

        // Apply access control if user context is provided
        if (user != null)
        {
            var filteredData = await ApplyAccessControlFilters(partners.AsQueryable(), user, "read");
            if (filteredData is IEnumerable<UNOPSPartner> partnerList)
            {
                partners = partnerList.ToList();
                _logger?.LogInformation("After RBAC filtering: {Count} partners remaining", partners.Count);
            }
            else
            {
                _logger?.LogWarning("ApplyAccessControlFilters returned unexpected type: {Type}", filteredData?.GetType().Name ?? "null");
            }
        }
        else
        {
            _logger?.LogInformation("No user context provided, skipping RBAC filtering");
        }

        // Process partners sequentially to avoid DbContext threading issues
        var results = new List<PartnerModel>();
        foreach (var partner in partners)
        {
            var mappedPartner = await MapEntityToModelAsync(partner, _mapper, user);
            results.Add(mappedPartner);
        }
        
        _logger?.LogInformation("Successfully mapped {Count} partners to models", results.Count);
        
        return results.Cast<object>().ToList();
    }
    
    /// <summary>
    /// Applies org unit filtering if the specification supports it using manual joins
    /// </summary>
    private IQueryable<UNOPSPartner> ApplyOrgUnitFilterIfSupported(IQueryable<UNOPSPartner> query, ISpecification<Partner> specification)
    {
        // Check if this is a PartnerSpecificationAdapter and get the original specification
        if (specification is PartnerSpecificationAdapter adapter)
        {
            var originalSpec = adapter.GetOriginalSpecification();
            return ApplyUNOPSPartnerOrgUnitFilterIfSupported(query, originalSpec);
        }
        
        // Check if specification has ApplyOrgUnitFilter method and call it
        var specType = specification.GetType();
        var filterMethod = specType.GetMethod("ApplyOrgUnitFilter", new[] { typeof(IQueryable<Partner>), typeof(DbContext) });
        
        if (filterMethod != null)
        {
            try
            {
                // Cast to base type for Partner specifications
                var baseQuery = query.Cast<Partner>();
                var result = filterMethod.Invoke(specification, new object[] { baseQuery, _context });
                if (result is IQueryable<Partner> filteredBaseQuery)
                {
                    return filteredBaseQuery.OfType<UNOPSPartner>();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue without org unit filtering
                Console.WriteLine($"Error applying org unit filter: {ex.Message}");
            }
        }
        
        // If no ApplyOrgUnitFilter method found, return original query
        return query;
    }
    
    /// <summary>
    /// Applies org unit filtering for UNOPS-specific specifications
    /// </summary>
    private IQueryable<UNOPSPartner> ApplyUNOPSPartnerOrgUnitFilterIfSupported(IQueryable<UNOPSPartner> query, ISpecification<UNOPSPartner> specification)
    {
        // Check if specification has ApplyOrgUnitFilter method and call it
        var specType = specification.GetType();
        var filterMethod = specType.GetMethod("ApplyOrgUnitFilter", new[] { typeof(IQueryable<UNOPSPartner>), typeof(DbContext) });
        
        if (filterMethod != null)
        {
            try
            {
                // Direct call for UNOPSPartner specifications
                var result = filterMethod.Invoke(specification, new object[] { query, _context });
                if (result is IQueryable<UNOPSPartner> filteredQuery)
                {
                    return filteredQuery;
                }
            }
            catch (Exception ex)
            {
                // Log error but continue without org unit filtering
                Console.WriteLine($"Error applying UNOPS partner org unit filter: {ex.Message}");
            }
        }
        
        // If no ApplyOrgUnitFilter method found, return original query
        return query;
    }

    #region Partner Status Management Methods

    /// <summary>
    /// Activates a draft partner after validating mandatory fields
    /// </summary>
    public async Task<PartnerModel?> ActivatePartnerAsync(ClaimsPrincipal user, int id, ActivatePartnerRequest request)
    {
        var entity = await PartnerRepository.GetByIdAsync(id, ["LiaisonOffice"]);
        if (entity == null)
            return null;

        // Check permissions through RBAC
        var hasAccess = await (_permissionService?.HasInstanceAccessAsync("Partner", entity, user, "update") ?? Task.FromResult(false));
        if (!hasAccess)
            return null;

        entity.ActivatePartner();
        await PartnerRepository.UpdateAsync(entity);
        
        // Load relationships and return updated model
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);
        var model = await MapEntityToModelAsync(entity, _mapper, user);
        return await MapEntityToModelWithPermissionsAsync(model, user, entity);
    }

    /// <summary>
    /// Closes an active partner (only for NotApproved partners)
    /// </summary>
    public async Task<PartnerModel?> ClosePartnerAsync(ClaimsPrincipal user, int id, StatusChangeRequest request)
    {
        var entity = await PartnerRepository.GetByIdAsync(id, ["LiaisonOffice"]);
        if (entity == null)
            return null;

        // Check permissions through RBAC
        var hasAccess = await (_permissionService?.HasInstanceAccessAsync("Partner", entity, user, "update") ?? Task.FromResult(false));
        if (!hasAccess)
            return null;

        // Additional validation: only NotApproved partners can be closed by regular users
        if (entity.PartnerApprovalStatus == PartnerApprovalStatus.Approved)
        {
            throw new UnauthorizedAccessException("Approved partners can only be closed by administrators.");
        }

        entity.ClosePartner();
        await PartnerRepository.UpdateAsync(entity);
        
        // Load relationships and return updated model
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);
        var model = await MapEntityToModelAsync(entity, _mapper, user);
        return await MapEntityToModelWithPermissionsAsync(model, user, entity);
    }

    /// <summary>
    /// Archives an active or closed partner (only for NotApproved partners)
    /// </summary>
    public async Task<PartnerModel?> ArchivePartnerAsync(ClaimsPrincipal user, int id, StatusChangeRequest request)
    {
        var entity = await PartnerRepository.GetByIdAsync(id, ["LiaisonOffice"]);
        if (entity == null)
            return null;

        // Check permissions through RBAC
        var hasAccess = await (_permissionService?.HasInstanceAccessAsync("Partner", entity, user, "update") ?? Task.FromResult(false));
        if (!hasAccess)
            return null;

        // Additional validation: only NotApproved partners can be archived by regular users
        if (entity.PartnerApprovalStatus == PartnerApprovalStatus.Approved)
        {
            throw new UnauthorizedAccessException("Approved partners can only be archived by administrators.");
        }

        entity.ArchivePartner();
        await PartnerRepository.UpdateAsync(entity);
        
        // Load relationships and return updated model
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);
        var model = await MapEntityToModelAsync(entity, _mapper, user);
        return await MapEntityToModelWithPermissionsAsync(model, user, entity);
    }

    /// <summary>
    /// Gets the next available ErpDimValue based on the highest existing value
    /// </summary>
    private async Task<int> GetNextErpDimValueAsync()
    {
        var highestErpDimValue = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && !p.IsDeleted)
            .MaxAsync(p => (int?)p.ErpDimValue) ?? 0;
        
        return highestErpDimValue + 1;
    }

    /// <summary>
    /// Approves an active partner (Admin only) - locks data fields and records approval audit trail
    /// </summary>
    public async Task<PartnerModel?> ApprovePartnerAsync(ClaimsPrincipal user, int id, UpdatePartnerRequest request)
    {
        var entity = await PartnerRepository.GetByIdAsync(id, ["LiaisonOffice"]);
        if (entity == null)
            return null;

        // Check if user has admin permissions for approval
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!userRoles.Contains("PARTNER_GLOB_ADMIN"))
        {
            throw new UnauthorizedAccessException("Only Partnership Global Administrators can approve partners.");
        }

        // Update all approval fields from the request before approving
        // PatchNonNullProperties now automatically excludes navigation properties like OrganizationUnitRelationships
        // PatchNonNullProperties now automatically handles string-to-enum conversion
        PatchNonNullProperties(request, entity);

        // Get user information for audit trail
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown Admin";

        // Get the next ErpDimValue for this partner
        var nextErpDimValue = await GetNextErpDimValueAsync();

        // Now approve the partner (this sets the approval status, audit trail, and ErpDimValue)
        entity.ApprovePartner(int.Parse(userId), userName, nextErpDimValue);
        await PartnerRepository.UpdateAsync(entity);
        
        // Load relationships and return updated model
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);
        var model = await MapEntityToModelAsync(entity, _mapper, user);
        return await MapEntityToModelWithPermissionsAsync(model, user, entity);
    }

    #endregion

    #region Partner Related Data Methods

    /// <summary>
    /// Gets all engagements for a specific partner with pagination
    /// </summary>
    public async Task<PaginationResponse<Engagement>> GetPartnerEngagementsAsync(ClaimsPrincipal user, int partnerId, int pageIndex, int pageSize, string? orderBy, bool ascending)
    {
        // Validate pagination parameters
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // First get the partner's ErpDimValue since Engagement.PartnerId references Partner.ErpDimValue, not Partner.Id
        var partner = await _context.Partners
            .Where(p => p.Id == partnerId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (partner == null || !partner.ErpDimValue.HasValue)
        {
            return new PaginationResponse<Engagement>
            {
                Records = new List<Engagement>(),
                TotalCount = 0,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = 0
            };
        }

        var baseQuery = _context.Engagements
            .Where(e => e.PartnerId == partner.ErpDimValue.Value && !e.IsDeleted)
            .Include(e => e.Partner);

        IQueryable<Engagement> query;
        
        // Apply ordering
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = ascending 
                ? baseQuery.OrderBy(e => EF.Property<object>(e, orderBy))
                : baseQuery.OrderByDescending(e => EF.Property<object>(e, orderBy));
        }
        else
        {
            // Default ordering by creation date (newest first)
            query = baseQuery.OrderByDescending(e => e.CreatedDate);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var engagements = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResponse<Engagement>
        {
            Records = engagements,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    /// <summary>
    /// Gets all projects for a specific partner with pagination
    /// </summary>
    public async Task<PaginationResponse<object>> GetPartnerProjectsAsync(ClaimsPrincipal user, int partnerId, int pageIndex, int pageSize, string? orderBy, bool ascending)
    {
        // Validate pagination parameters
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // Get the partner first to access the projects through the many-to-many relationship
        var partner = await _context.Partners
            .Include(p => p.Projects)
            .ThenInclude(proj => proj.Partners) // Include partners for each project
            .FirstOrDefaultAsync(p => p.Id == partnerId && !p.IsDeleted);

        if (partner == null)
        {
            throw new ArgumentException("Partner not found or access denied.");
        }

        // Get the projects associated with this partner
        var projectsQuery = partner.Projects.AsQueryable()
            .Where(p => p.Status == EntityStatus.Active);

        // Apply ordering
        if (!string.IsNullOrEmpty(orderBy))
        {
            projectsQuery = ascending 
                ? projectsQuery.OrderBy(p => EF.Property<object>(p, orderBy))
                : projectsQuery.OrderByDescending(p => EF.Property<object>(p, orderBy));
        }
        else
        {
            // Default ordering by start date (newest first)
            projectsQuery = projectsQuery.OrderByDescending(p => p.StartDate);
        }

        var totalCount = projectsQuery.Count();

        // Apply pagination
        var projects = projectsQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Map to anonymous objects to avoid ProjectModel dependency
        var projectModels = projects.Select(p => (object)new {
            p.Id,
            p.ProjectNumber,
            p.Name,
            p.StartDate,
            p.EndDate,
            p.Stage,
            p.BudgetCheckingLevel,
            p.BudgetDuration,
            p.BudgetAmount,
            p.ExpenditureAmount
        }).ToList();

        return new PaginationResponse<object>
        {
            Records = projectModels,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    #endregion

    /// <summary>
    /// Gets a partner by name (case-insensitive search)
    /// </summary>
    /// <param name="user">The current user's claims principal</param>
    /// <param name="name">The partner name to search for</param>
    /// <returns>The partner model if found and user has access, null otherwise</returns>
    public async Task<PartnerModel?> GetPartnerByNameAsync(ClaimsPrincipal user, string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            // Query for partner with the specified name (case-insensitive)
            var partner = await _context.Partners
                .Where(p => p.Name.ToLower() == name.ToLower() && !p.IsDeleted)
                .Include(p => p.PartnerGroup)
                .AsQueryable()
                .FirstOrDefaultAsync();

            if (partner == null)
            {
                return null;
            }

            // Load organization unit relationships
            await partner.LoadOrganizationUnitRelationshipsAsync(_context);

            // Apply access control filters to ensure user has permission to access this partner
            var query = _context.Partners
                .Where(p => p.Id == partner.Id)
                .Include(p => p.PartnerGroup)
                .AsQueryable();

            var filteredData = await ApplyAccessControlFilters(query, user, "read");
            
            if (filteredData is IEnumerable<UNOPSPartner> partnerList)
            {
                var accessiblePartner = partnerList.FirstOrDefault();
                if (accessiblePartner != null)
                {
                    // Load relationships again for the filtered partner
                    await accessiblePartner.LoadOrganizationUnitRelationshipsAsync(_context);
                    var model = await MapEntityToModelAsync(accessiblePartner, _mapper, user);
                    return await MapEntityToModelWithPermissionsAsync(model, user, accessiblePartner);
                }
            }

            // User doesn't have access to this partner
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting partner by name: {Name}", name);
            return null;
        }
    }

    /// <summary>
    /// Performs comprehensive smart search across Partners and all related entities.
    /// Searches through partner information, contacts, partner groups, liaison offices, 
    /// organization units, and applies intelligent ranking based on relevance.
    /// </summary>
    /// <param name="user">The user performing the search (for RBAC)</param>
    /// <param name="searchText">Text to search across all partner and related entity fields</param>
    /// <param name="includeInactive">Whether to include inactive/deleted partners (default: false)</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <param name="request">Pagination request for final result formatting</param>
    /// <returns>Paginated response with ranked search results and metadata</returns>
    public async Task<PaginationResponse<PartnerModel>> PerformSmartSearchAsync(
        ClaimsPrincipal user,
        string searchText,
        bool includeInactive = false,
        int maxResults = 50,
        PaginationRequest? request = null)
    {
        _logger?.LogInformation("Starting smart search for: '{SearchText}' (includeInactive: {IncludeInactive}, maxResults: {MaxResults})", 
            searchText, includeInactive, maxResults);

        try
        {
            // Use the base smart search functionality
            var smartSearchResult = await PerformSmartSearchAsync<UNOPSPartner>(
                searchText, 
                includeInactive, 
                maxResults);

            // Extract just the Partner entities from the smart search results
            var partnerEntities = smartSearchResult.Results.Select(r => r.Entity).ToList();

            // Apply RBAC filtering
            var accessiblePartners = await FilterAccessiblePartners(partnerEntities, user);

            // Map to PartnerModels with full permissions
            var partnerModels = new List<PartnerModel>();
            foreach (var partner in accessiblePartners)
            {
                var model = await MapEntityToModelAsync(partner, _mapper, user);
                var modelWithPermissions = await MapEntityToModelWithPermissionsAsync(model, user, partner);
                partnerModels.Add(modelWithPermissions);
            }

            // Create pagination response
            var paginationRequest = request ?? new PaginationRequest { PageIndex = 0, PageSize = maxResults };
            var totalCount = partnerModels.Count;
            var pageIndex = Math.Max(0, paginationRequest.PageIndex);
            var pageSize = Math.Max(1, Math.Min(paginationRequest.PageSize, maxResults));
            
            var pagedResults = partnerModels
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            _logger?.LogInformation("Smart search completed: Found {TotalResults} partners in {ExecutionTime}ms. Strategy: {Strategy}", 
                totalCount, smartSearchResult.ExecutionTime.TotalMilliseconds, smartSearchResult.SearchStrategy);

            return new PaginationResponse<PartnerModel>
            {
                Records = pagedResults,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error performing smart search for: '{SearchText}'", searchText);
            
            // Return empty result on error
            return new PaginationResponse<PartnerModel>
            {
                Records = new List<PartnerModel>(),
                TotalCount = 0,
                PageIndex = 0,
                PageSize = maxResults,
                TotalPages = 0
            };
        }
    }

    /// <summary>
    /// Debug method to get total partner count
    /// </summary>
    public async Task<int> GetTotalPartnerCountAsync(ClaimsPrincipal user)
    {
        return await _context.Partners.CountAsync();
    }

    /// <summary>
    /// Debug method to get sample partner names
    /// </summary>
    public async Task<List<string>> GetSamplePartnerNamesAsync(ClaimsPrincipal user, int count = 5)
    {
        try
        {
            return await _context.Partners
                .Take(count)
                .Select(p => p.Name)
                .ToListAsync();
        }
        catch
        {
            return new List<string> { "Error retrieving sample names" };
        }
    }


    // GetPartnerSearchFields removed - now handled directly in PartnerController with translation keys for multilingual support

    /// <summary>
    /// Filters the list of partners based on user's RBAC permissions
    /// </summary>
    private async Task<List<UNOPSPartner>> FilterAccessiblePartners(List<UNOPSPartner> partners, ClaimsPrincipal user)
    {
        try
        {
            var accessiblePartners = new List<UNOPSPartner>();
            
            foreach (var partner in partners)
            {
                // Apply access control filters to ensure user has permission to access this partner
                var query = _context.Partners
                    .Where(p => p.Id == partner.Id)
                    .Include(p => p.PartnerGroup)
                    .AsQueryable();

                var filteredData = await ApplyAccessControlFilters(query, user, "read");
                
                if (filteredData is IEnumerable<UNOPSPartner> partnerList && partnerList.Any())
                {
                    // Load relationships for accessible partners
                    await partner.LoadOrganizationUnitRelationshipsAsync(_context);
                    accessiblePartners.Add(partner);
                }
            }
            
            return accessiblePartners;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error filtering accessible partners, returning empty list");
            return new List<UNOPSPartner>();
        }
    }

}