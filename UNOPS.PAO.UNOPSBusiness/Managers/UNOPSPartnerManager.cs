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
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using UNOPS.PAO.UNOPSBusiness.Extensions;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Integrations;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Contacts;

public class UNOPSPartnerManager : BaseUNOPSManager, IPartnerManager
{
    private new readonly IMapper _mapper;
    private new readonly UNOPSAppDbContext _context;
    private new readonly IConfiguration _configuration;
    private readonly ILogger<UNOPSPartnerManager> _logger;
    private readonly GlobalFilterService? _globalFilterService;
    private readonly IDbContextFactory<UNOPSAppDbContext>? _dbContextFactory;

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

        // Resolve user names for audit fields
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.CreatedBy != 0)
        {
            result.CreatedByName = await GetUserNameByIdAsync(entity.CreatedBy);
        }
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.LastModifiedBy != 0)
        {
            result.LastModifiedByName = await GetUserNameByIdAsync(entity.LastModifiedBy);
        }

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

        // Populate PartnerFocalPointUserName and PartnerFocalPointName if PartnerFocalPointUserId exists
        if (result.PartnerFocalPointUserId.HasValue && result.PartnerFocalPointUserId.Value > 0)
        {
            var focalPointUser = await _context.PAOUsers
                .Include(u => u.UserProfile)
                .Where(u => u.Id == result.PartnerFocalPointUserId.Value)
                .FirstOrDefaultAsync();
            if (focalPointUser != null)
            {
                result.PartnerFocalPointUserName = focalPointUser.Email; // Username is the email
                result.PartnerFocalPointName = !string.IsNullOrEmpty(focalPointUser.Name) ? focalPointUser.Name : focalPointUser.Email; // Display name
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

    public UNOPSPartnerManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService, ILogger<UNOPSPartnerManager> logger, IPermissionService permissionService, GlobalFilterService? globalFilterService, IHttpContextAccessor httpContextAccessor = null, IServiceProvider serviceProvider = null, IDbContextFactory<UNOPSAppDbContext>? dbContextFactory = null)
        : base(mapper, context, configuration, null, "Partner", permissionService, httpContextAccessor)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _globalFilterService = globalFilterService;
        _dbContextFactory = dbContextFactory;
       // _securityService = securityService;
        PartnerRepository = new BaseRepository<UNOPSPartner>(context, configuration, serviceProvider);
        PartnerTreeRepository = new BaseRepository<UNOPSPartnerTree>(context, configuration, serviceProvider);
        OrganizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration, serviceProvider);
        
        PartnerTreeService = partnerTreeService;
        
        GoogleCloudStorageService = new GoogleCloudStorageService(configuration);

        commonRepository = new CommonEntityRepository(context);
    }

    private async Task<string> GetUserNameByIdAsync(int userId)
    {
        try
        {
            var userProfile = await _context.UserProfile.FirstOrDefaultAsync(up => up.UserId == userId);
            if (userProfile != null && !string.IsNullOrEmpty(userProfile.Name))
            {
                return userProfile.Name;
            }
            
            // Fallback to PAOUser email if UserProfile not found or Name is empty
            var user = await _context.PAOUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                return user.Email;
            }
        }
        catch (Exception)
        {
            // Log error if needed, but don't fail the entire operation
        }
        
        return $"User #{userId}";
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
        // ==========================================
        // OPTIMIZATION: Use _context.Partners directly for IQueryable with AsNoTracking()
        // Split query to avoid Cartesian product with collections
        // ==========================================
        var query = _context.Partners
            .AsNoTracking() // Read-only optimization
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        
        // Get the entities for this page (IDs only for now)
        var partnerIds = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .Select(p => p.Id)
            .ToListAsync();

        // ==========================================
        // BATCH QUERY: Load all partners with their navigation properties in separate queries
        // This eliminates N+1 and Cartesian product issues
        // ==========================================
        
        // Load partners with PartnerGroup
        var partners = await _context.Partners
            .AsNoTracking()
            .Where(p => partnerIds.Contains(p.Id))
            .Include(p => p.PartnerGroup)
            .ToListAsync();

        // Load contacts separately for these partners (batch query)
        var allContacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => partnerIds.Contains(c.PartnerId))
            .ToListAsync();

        // Group contacts by partner ID for efficient assignment
        var contactsByPartner = allContacts
            .GroupBy(c => c.PartnerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Assign contacts to partners
        foreach (var partner in partners)
        {
            if (contactsByPartner.TryGetValue(partner.Id, out var contacts))
            {
                partner.Contacts = contacts.Cast<Contact>().ToList();
            }
        }

        // Load organization unit relationships
        await partners.LoadOrganizationUnitRelationshipsAsync(_context);

        // Map entities asynchronously
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in partners)
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
        // ==========================================
        // OPTIMIZATION: Use _context.Partners directly for IQueryable with AsNoTracking()
        // ==========================================
        var query = _context.Partners
            .AsNoTracking() // Read-only optimization
            .AsQueryable();

        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Partner>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSPartner>();
        
        // Apply global filters using the centralized GlobalFilterService
        if (_globalFilterService != null && pagination.FilterActive == true)
        {
            filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(filteredQuery, GetCurrentUserOrSystemContext());
        }
        
        // Get total count
        var totalCount = await filteredQuery.CountAsync();
        
        // Apply pagination
        var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        var excludedRows = (pageIndex - 1) * pagination.PageSize;
        
        if (pagination.OrderBy != null)
        {
            filteredQuery = filteredQuery.OrderByColumnName(pagination.OrderBy, pagination.Ascending ?? true);
        }
        
        // Get partner IDs for this page
        var partnerIds = await filteredQuery
            .Skip(excludedRows)
            .Take(pagination.PageSize)
            .Select(p => p.Id)
            .ToListAsync();

        // ==========================================
        // BATCH QUERY: Load partners and contacts separately to avoid Cartesian product
        // ==========================================
        var partners = await _context.Partners
            .AsNoTracking()
            .Where(p => partnerIds.Contains(p.Id))
            .ToListAsync();

        // Load contacts separately for these partners (batch query)
        var allContacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => partnerIds.Contains(c.PartnerId))
            .ToListAsync();

        // Group contacts by partner ID for efficient assignment
        var contactsByPartner = allContacts
            .GroupBy(c => c.PartnerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Assign contacts to partners
        foreach (var partner in partners)
        {
            if (contactsByPartner.TryGetValue(partner.Id, out var contacts))
            {
                partner.Contacts = contacts.Cast<Contact>().ToList();
            }
        }

        // Load organization unit relationships
        await partners.LoadOrganizationUnitRelationshipsAsync(_context);

        // Map entities asynchronously with default permissions
        var mappedEntities = new List<PartnerModel>();
        foreach (var entity in partners)
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
        
        // Apply global filters using the centralized GlobalFilterService
        if (_globalFilterService != null && pagination.FilterActive == true)
        {
            filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(filteredQuery, user);
        }
        
        // Apply access control filters (role-based permissions only) BEFORE pagination
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
        // ==========================================
        // OPTIMIZATION: Use AsNoTracking() for read-only query
        // ==========================================
        var item = await _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (item == null)
        {
            return default;
        }

        // Load organization unit relationships for single partner
        await item.LoadOrganizationUnitRelationshipsAsync(_context);

        return await MapEntityToModelAsync(item, _mapper, null);
    }


    /// <summary>
    /// Gets comprehensive partner details formatted for AI prompt processing
    /// OPTIMIZED: Uses split queries with AsNoTracking() for better performance
    /// </summary>
    public async Task<object> GetBasicPartnerDetailsAsync(ClaimsPrincipal user, int id)
    {
        // ==========================================
        // OPTIMIZATION: Split query - load main entity first, then collections separately
        // Note: Documents use navigation property, not direct FK, so we load via Include
        // ==========================================
        var entity = await _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .Include(p => p.Documents) // Documents must be loaded via Include (navigation property)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) 
        {
            return new { error = "Partner not found" };
        }

        // ==========================================
        // QUERY 2: Load contacts separately (eliminates Cartesian product)
        // Filter out soft-deleted records
        // ==========================================
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == id && !c.IsDeleted)
            .ToListAsync();

        // Assign contacts to entity
        entity.Contacts = contacts.Cast<Contact>().ToList();

        // Load organization unit relationships
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);

        // ==========================================
        // BATCH QUERY: Load interactions for all contacts in one query (eliminates N+1)
        // ==========================================
        if (contacts != null && contacts.Any())
        {
            var contactIds = contacts.Select(c => c.Id).ToList();
            
            // Get recent interactions through the junction table in one batch query
            var interactionContacts = await _context.InteractionContacts
                .AsNoTracking()
                .Where(ic => contactIds.Contains(ic.ContactId))
                .Include(ic => ic.Interaction)
                .Where(ic => ic.Interaction.Date >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            // Group interactions by contact for efficient assignment
            var interactionsByContact = interactionContacts
                .GroupBy(ic => ic.ContactId)
                .ToDictionary(g => g.Key, g => g.Select(ic => ic.Interaction).ToList());

            // Assign interactions to each contact (in-memory operation)
            foreach (var contact in contacts)
            {
                if (interactionsByContact.TryGetValue(contact.Id, out var interactions))
                {
                    contact.Interactions = interactions;
                }
            }
        }

        // Create comprehensive JSON for AI prompt placeholders
        var result = new
        {
            id = entity.Id,
            name = entity.Name,
            partnerName = entity.Name, // Alias for prompt compatibility
            status = entity.Status.ToString(),
            description = entity.Name, // Partners don't have a separate description field
            
            // Partner group information
            partnerGroup = entity.PartnerGroup != null ? new
            {
                id = entity.PartnerGroup.Id,
                name = entity.PartnerGroup.Name,
                code = entity.PartnerGroup.Code,
                type = entity.PartnerGroup.Type?.ToString()
            } : null,
            
            // Liaison office information
            liaisonOffice = entity.LiaisonOffice != null ? new
            {
                id = entity.LiaisonOffice.Id,
                name = entity.LiaisonOffice.Name,
                code = entity.LiaisonOffice.Code
            } : null,
            
            // Contact information
            contacts = entity.Contacts?.Select(c => new
            {
                id = c.Id,
                fullName = $"{c.FirstName} {c.LastName}".Trim(),
                firstName = c.FirstName,
                lastName = c.LastName,
                title = c.Title,
                email = c.Email,
                phone = c.Phone,
                mobile = c.Mobile,
                department = c.Department,
                status = c.Status.ToString(),
                totalInteractions = c.Interactions?.Count ?? 0
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Recent interactions (through contacts)
            recentInteractions = entity.Contacts?
                .SelectMany(c => c.Interactions ?? new List<Interaction>())
                .Where(i => i.Date >= DateTime.UtcNow.AddDays(-30))
                .OrderByDescending(i => i.Date)
                .Take(10)
                .Select(i => new
                {
                    id = i.Id,
                    subject = i.Subject,
                    description = i.Description,
                    date = i.Date.ToString("yyyy-MM-dd HH:mm"),
                    type = i.Type.ToString(),
                    location = i.Location
                }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Documents and attachments
            documents = entity.Documents?.Select(d => new
            {
                id = d.Id,
                link = d.Link,
                type = d.Type,
                documentType = d.DocumentType?.Name,
                uploadDate = d.CreatedDate.ToString("yyyy-MM-dd"),
                downloadUrl = d.Link
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Organization unit relationships
            organizationUnits = entity.OrganizationUnitRelationships?.Select(our => new
            {
                id = our.OrganizationHierarchyId,
                name = our.OrganizationHierarchy?.Name ?? "Organization Unit",
                code = our.OrganizationHierarchy?.Code ?? our.OrganizationHierarchyId.ToString(),
                type = our.OrganizationHierarchy?.Type.ToString() ?? "OrgUnit"
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Summary statistics
            summary = new
            {
                totalContacts = entity.Contacts?.Count ?? 0,
                activeContacts = entity.Contacts?.Count(c => c.Status == Domain.Entities.EntityStatus.Active) ?? 0,
                totalDocuments = entity.Documents?.Count ?? 0,
                totalInteractions = entity.Contacts?.SelectMany(c => c.Interactions ?? new List<Interaction>()).Count() ?? 0,
                recentInteractions = entity.Contacts?
                    .SelectMany(c => c.Interactions ?? new List<Interaction>())
                    .Count(i => i.Date >= DateTime.UtcNow.AddDays(-30)) ?? 0,
                lastInteractionDate = entity.Contacts?
                    .SelectMany(c => c.Interactions ?? new List<Interaction>())
                    .OrderByDescending(i => i.Date)
                    .FirstOrDefault()?.Date.ToString("yyyy-MM-dd")
            },
            
            // Partnership details
            partnership = new
            {
                partnershipLevel = entity.PartnerGroup?.Name ?? "Not specified",
                primaryContact = entity.Contacts?.FirstOrDefault(c => c.Status == Domain.Entities.EntityStatus.Active),
                establishedDate = entity.CreatedDate.ToString("yyyy-MM-dd"),
                lastActivity = entity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not available"
            },
            
            // Audit information
            auditInfo = new
            {
                createdDate = entity.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = entity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not modified",
                createdBy = entity.CreatedBy,
                lastModifiedBy = entity.LastModifiedBy
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

        return result;
    }

    /// <summary>   
    /// Gets a partner with its contacts and their interactions included
    /// OPTIMIZED: Uses split queries with AsNoTracking() for better performance
    /// </summary>
    public async Task<PartnerModel?> GetPartnerWithContactsAndInteractionsAsync(int id)
    {
        // ==========================================
        // OPTIMIZATION: Documents loaded via Include, Contacts separately
        // ==========================================
        var partner = await _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
            .Include(p => p.Documents) // Documents must be loaded via Include (navigation property)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (partner == null)
        {
            return default;
        }

        // ==========================================
        // QUERY 2: Load contacts separately to avoid Cartesian product
        // Filter out soft-deleted records
        // ==========================================
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == id && !c.IsDeleted)
            .ToListAsync();

        // Assign contacts to partner
        partner.Contacts = contacts.Cast<Contact>().ToList();

        // Load organization unit relationships for single partner
        await partner.LoadOrganizationUnitRelationshipsAsync(_context);

        // ==========================================
        // BATCH QUERY: Load interactions for all contacts in one query (eliminates N+1)
        // ==========================================
        if (contacts != null && contacts.Any())
        {
            var contactIds = contacts.Select(c => c.Id).ToList();
            
            // Get interactions through the junction table in one batch query
            var interactionContacts = await _context.InteractionContacts
                .AsNoTracking()
                .Where(ic => contactIds.Contains(ic.ContactId))
                .Include(ic => ic.Interaction)
                .ToListAsync();

            // Group interactions by contact for efficient assignment
            var interactionsByContact = interactionContacts
                .GroupBy(ic => ic.ContactId)
                .ToDictionary(g => g.Key, g => g.Select(ic => ic.Interaction).ToList());

            // Assign interactions to each contact (in-memory operation)
            foreach (var contact in contacts)
            {
                if (interactionsByContact.TryGetValue(contact.Id, out var interactions))
                {
                    contact.Interactions = interactions;
                }
            }
        }

        return await MapEntityToModelAsync(partner, _mapper, null);
    }

    /// <summary>
    /// Gets comprehensive partner with contacts and interactions formatted for AI prompt processing
    /// OPTIMIZED: Uses split queries with AsNoTracking() and parallel execution for better performance
    /// </summary>
    public async Task<object> GetPartnerWithContactsAndInteractionsForAIAsync(ClaimsPrincipal user, int id)
    {
        // ==========================================
        // OPTIMIZATION: Split query with optional parallel execution
        // Documents use navigation property, so loaded via Include
        // Contacts loaded separately to avoid Cartesian product
        // ==========================================
        var entity = await _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .Include(p => p.Documents) // Documents must be loaded via Include (navigation property)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) 
        {
            return new { error = "Partner not found" };
        }

        // ==========================================
        // QUERY 2: Load contacts separately (can be parallelized if needed)
        // Filter out soft-deleted records
        // ==========================================
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == id && !c.IsDeleted)
            .ToListAsync();

        // Assign contacts to entity
        entity.Contacts = contacts.Cast<Contact>().ToList();

        // Load organization unit relationships
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);

        // ==========================================
        // BATCH QUERY: Load interactions for all contacts in one query (eliminates N+1)
        // ==========================================
        if (contacts != null && contacts.Any())
        {
            var contactIds = contacts.Select(c => c.Id).ToList();
            
            // Get interactions through the junction table in one batch query
            var interactionContacts = await _context.InteractionContacts
                .AsNoTracking()
                .Where(ic => contactIds.Contains(ic.ContactId))
                .Include(ic => ic.Interaction)
                .ToListAsync();

            // Group interactions by contact for efficient assignment
            var interactionsByContact = interactionContacts
                .GroupBy(ic => ic.ContactId)
                .ToDictionary(g => g.Key, g => g.Select(ic => ic.Interaction).ToList());

            // Assign interactions to each contact (in-memory operation)
            foreach (var contact in contacts)
            {
                if (interactionsByContact.TryGetValue(contact.Id, out var interactions))
                {
                    contact.Interactions = interactions;
                }
            }
        }

        // Create comprehensive JSON for AI prompt placeholders
        var result = new
        {
            id = entity.Id,
            name = entity.Name,
            partnerName = entity.Name, // Alias for prompt compatibility
            status = entity.Status.ToString(),
            description = entity.Name, // Partners don't have a separate description field
            
            // Partner group information
            partnerGroup = entity.PartnerGroup != null ? new
            {
                id = entity.PartnerGroup.Id,
                name = entity.PartnerGroup.Name,
                code = entity.PartnerGroup.Code,
                type = entity.PartnerGroup.Type?.ToString()
            } : null,
            
            // Liaison office information
            liaisonOffice = entity.LiaisonOffice != null ? new
            {
                id = entity.LiaisonOffice.Id,
                name = entity.LiaisonOffice.Name,
                code = entity.LiaisonOffice.Code
            } : null,
            
            // Comprehensive contact information with interactions
            contacts = entity.Contacts?.Select(c => new
            {
                id = c.Id,
                fullName = $"{c.FirstName} {c.LastName}".Trim(),
                firstName = c.FirstName,
                lastName = c.LastName,
                title = c.Title,
                email = c.Email,
                phone = c.Phone,
                mobile = c.Mobile,
                department = c.Department,
                status = c.Status.ToString(),
                
                // Contact's interactions
                interactions = c.Interactions?.Select(i => new
                {
                    id = i.Id,
                    subject = i.Subject,
                    description = i.Description,
                    date = i.Date.ToString("yyyy-MM-dd HH:mm"),
                    type = i.Type.ToString(),
                    location = i.Location,
                    status = "Active" // Default status for interactions
                }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
                
                totalInteractions = c.Interactions?.Count ?? 0,
                recentInteractions = c.Interactions?.Where(i => i.Date >= DateTime.UtcNow.AddDays(-30)).Count() ?? 0,
                lastInteractionDate = c.Interactions?.OrderByDescending(i => i.Date).FirstOrDefault()?.Date.ToString("yyyy-MM-dd")
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // All interactions (flattened from contacts)
            allInteractions = entity.Contacts?
                .SelectMany(c => c.Interactions ?? new List<Interaction>())
                .OrderByDescending(i => i.Date)
                .Select(i => new
                {
                    id = i.Id,
                    subject = i.Subject,
                    description = i.Description,
                    date = i.Date.ToString("yyyy-MM-dd HH:mm"),
                    type = i.Type.ToString(),
                    location = i.Location,
                    contactName = entity.Contacts?.FirstOrDefault(c => c.Interactions?.Contains(i) == true)?.FirstName + " " + 
                                  entity.Contacts?.FirstOrDefault(c => c.Interactions?.Contains(i) == true)?.LastName
                }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Recent interactions (last 30 days)
            recentInteractions = entity.Contacts?
                .SelectMany(c => c.Interactions ?? new List<Interaction>())
                .Where(i => i.Date >= DateTime.UtcNow.AddDays(-30))
                .OrderByDescending(i => i.Date)
                .Take(10)
                .Select(i => new
                {
                    id = i.Id,
                    subject = i.Subject,
                    description = i.Description,
                    date = i.Date.ToString("yyyy-MM-dd HH:mm"),
                    type = i.Type.ToString(),
                    location = i.Location,
                    contactName = entity.Contacts?.FirstOrDefault(c => c.Interactions?.Contains(i) == true)?.FirstName + " " + 
                                  entity.Contacts?.FirstOrDefault(c => c.Interactions?.Contains(i) == true)?.LastName
                }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Documents and attachments
            documents = entity.Documents?.Select(d => new
            {
                id = d.Id,
                link = d.Link,
                type = d.Type,
                documentType = d.DocumentType?.Name,
                uploadDate = d.CreatedDate.ToString("yyyy-MM-dd"),
                downloadUrl = d.Link
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Organization unit relationships
            organizationUnits = entity.OrganizationUnitRelationships?.Select(our => new
            {
                id = our.OrganizationHierarchyId,
                name = our.OrganizationHierarchy?.Name ?? "Organization Unit",
                code = our.OrganizationHierarchy?.Code ?? our.OrganizationHierarchyId.ToString(),
                type = our.OrganizationHierarchy?.Type.ToString() ?? "OrgUnit"
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Comprehensive summary statistics
            summary = new
            {
                totalContacts = entity.Contacts?.Count ?? 0,
                activeContacts = entity.Contacts?.Count(c => c.Status == Domain.Entities.EntityStatus.Active) ?? 0,
                totalDocuments = entity.Documents?.Count ?? 0,
                totalInteractions = entity.Contacts?.SelectMany(c => c.Interactions ?? new List<Interaction>()).Count() ?? 0,
                recentInteractions = entity.Contacts?
                    .SelectMany(c => c.Interactions ?? new List<Interaction>())
                    .Count(i => i.Date >= DateTime.UtcNow.AddDays(-30)) ?? 0,
                lastInteractionDate = entity.Contacts?
                    .SelectMany(c => c.Interactions ?? new List<Interaction>())
                    .OrderByDescending(i => i.Date)
                    .FirstOrDefault()?.Date.ToString("yyyy-MM-dd"),
                mostActiveContact = entity.Contacts?
                    .OrderByDescending(c => c.Interactions?.Count ?? 0)
                    .FirstOrDefault()?.FirstName + " " + 
                    entity.Contacts?
                    .OrderByDescending(c => c.Interactions?.Count ?? 0)
                    .FirstOrDefault()?.LastName,
                averageInteractionsPerContact = entity.Contacts?.Count > 0 ? 
                    (entity.Contacts.SelectMany(c => c.Interactions ?? new List<Interaction>()).Count() / (double)entity.Contacts.Count) : 0
            },
            
            // Partnership engagement analysis
            engagement = new
            {
                partnershipLevel = entity.PartnerGroup?.Name ?? "Not specified",
                engagementFrequency = entity.Contacts?
                    .SelectMany(c => c.Interactions ?? new List<Interaction>())
                    .Count(i => i.Date >= DateTime.UtcNow.AddDays(-90)) > 5 ? "High" : "Low",
                keyContactPoints = entity.Contacts?
                    .Where(c => (c.Interactions?.Count ?? 0) > 0)
                    .Select(c => c.FirstName + " " + c.LastName)
                    .ToList() ?? new List<string>(),
                lastEngagementType = entity.Contacts?
                    .SelectMany(c => c.Interactions ?? new List<Interaction>())
                    .OrderByDescending(i => i.Date)
                    .FirstOrDefault()?.Type.ToString()
            },
            
            // Audit information
            auditInfo = new
            {
                createdDate = entity.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = entity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not modified",
                createdBy = entity.CreatedBy,
                lastModifiedBy = entity.LastModifiedBy
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

        return result;
    }

    /// <summary>
    /// Gets partner risk profile with comprehensive details - designed for risk analysis and AI prompts
    /// OPTIMIZED: Uses split queries with AsNoTracking() for better performance
    /// </summary>
    public async Task<PartnerModel?> GetPartnerRiskProfileAsync(int id)
    {
        // ==========================================
        // OPTIMIZATION: Documents loaded via Include, Contacts separately
        // ==========================================
        var partner = await _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
            .Include(p => p.Documents) // Documents must be loaded via Include (navigation property)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (partner == null)
        {
            return default;
        }

        // ==========================================
        // QUERY 2: Load contacts separately to avoid Cartesian product
        // Filter out soft-deleted records
        // ==========================================
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == id && !c.IsDeleted)
            .ToListAsync();

        // Assign contacts to partner
        partner.Contacts = contacts.Cast<Contact>().ToList();

        // Load organization unit relationships for single partner
        await partner.LoadOrganizationUnitRelationshipsAsync(_context);

        // ==========================================
        // BATCH QUERY: Load interactions for all contacts in one query (eliminates N+1)
        // ==========================================
        if (contacts != null && contacts.Any())
        {
            var contactIds = contacts.Select(c => c.Id).ToList();
            
            // Get interactions through the junction table in one batch query
            var interactionContacts = await _context.InteractionContacts
                .AsNoTracking()
                .Where(ic => contactIds.Contains(ic.ContactId))
                .Include(ic => ic.Interaction)
                .ToListAsync();

            // Group interactions by contact for efficient assignment
            var interactionsByContact = interactionContacts
                .GroupBy(ic => ic.ContactId)
                .ToDictionary(g => g.Key, g => g.Select(ic => ic.Interaction).ToList());

            // Assign interactions to each contact (in-memory operation)
            foreach (var contact in contacts)
            {
                if (interactionsByContact.TryGetValue(contact.Id, out var interactions))
                {
                    contact.Interactions = interactions;
                }
            }
        }

        var result = await MapEntityToModelAsync(partner, _mapper, null);

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

            return await GoogleCloudStorageService.GenerateSignedUrlFromStorageUrl(publicUrl);
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
        var item = await PartnerRepository.GetByIdAsync(id, ["PartnerGroup", "LiaisonOffice"]);
        if (item == null)
        {
            return null;
        }

        // Check if user has permission to access this specific entity
        // Create a single-item query and apply access control filters
        var query = PartnerRepository
            .GetAll(["PartnerGroup", "LiaisonOffice"])
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

        // Validate Partner Levy business rules
        if (model.PartnerLevyStatus == "DoesNotApply" || model.PartnerLevyStatus == "PotentiallyNotApplied")
        {
            if (string.IsNullOrWhiteSpace(model.ReasonForLevy))
            {
                throw new BusinessException("Reason for Levy is required when Partner Levy status is 'Does Not Apply' or 'Potentially Not Applied'.");
            }
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

        // Validate Partner Levy business rules
        if (model.PartnerLevyStatus == "DoesNotApply" || model.PartnerLevyStatus == "PotentiallyNotApplied")
        {
            if (string.IsNullOrWhiteSpace(model.ReasonForLevy))
            {
                throw new BusinessException("Reason for Levy is required when Partner Levy status is 'Does Not Apply' or 'Potentially Not Applied'.");
            }
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

        // Soft delete associated OrganizationUnitRelationship records
        await SoftDeleteOrganizationUnitRelationshipsAsync(id, "Partner");

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
        // ==========================================
        // OPTIMIZATION: Documents loaded via Include, Contacts separately
        // ==========================================
        var item = await _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
            .Include(p => p.Documents) // Documents must be loaded via Include (navigation property)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (item == null)
        {
            return default;
        }

        // Load contacts separately to avoid Cartesian product
        // Filter out soft-deleted records
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == id && !c.IsDeleted)
            .ToListAsync();

        // Assign contacts
        item.Contacts = contacts.Cast<Contact>().ToList();

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
            // Soft delete associated OrganizationUnitRelationship records
            await SoftDeleteOrganizationUnitRelationshipsAsync(id, "Partner");
            
            await PartnerRepository.Delete(entity);
        }
    }

    public async Task<PartnerModel?> GetPartnerAsync(int userId, int id)
    {
        // Use the original implementation but fix to match the interface
        return await GetPartnerAsync(id);
    }

    /// <summary>
    /// Gets partners for Gmail addon with contacts and interactions
    /// OPTIMIZED: Uses AsNoTracking() and batch queries to eliminate N+1 patterns
    /// </summary>
    public async Task<List<PartnerModel?>> GetPartnersForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user = null)
    {
        // ==========================================
        // QUERY 1: Load partners with AsNoTracking() for read-only operation
        // ==========================================
        var partners = await _context.Partners
            .AsNoTracking()
            .Where(p => input.partnerIds.Contains(p.Id))
            .Include(p => p.PartnerGroup)
            .ToListAsync();

        // Load organization unit relationships manually
        await partners.LoadOrganizationUnitRelationshipsAsync(_context);

        // Get all partner IDs to load interactions and contacts
        var allPartnerIds = partners.Select(p => p.Id).ToList();

        // ==========================================
        // BATCH QUERY: Load all contacts for these partners in one query
        // ==========================================
        var allContacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => allPartnerIds.Contains(c.PartnerId))
            .Cast<UNOPSContact>()
            .ToListAsync();

        // Group contacts by partner ID for efficient lookup
        var contactsByPartner = allContacts
            .GroupBy(c => c.PartnerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ==========================================
        // BATCH QUERY: Load all interactions for these partners in one query
        // ==========================================
        var interactionPartners = await _context.InteractionPartners
            .AsNoTracking()
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
    /// OPTIMIZED: Uses AsNoTracking() for read-only query
    /// </summary>
    public override async Task<object> GetBasicEntityDataAsync(int id)
    {
        var partner = await _context.Partners
            .AsNoTracking()
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
    /// OPTIMIZED: Uses AsNoTracking() for read-only query
    /// </summary>
    public override async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        if (ids == null || ids.Length == 0)
            return new List<object>();

        _logger?.LogInformation("UNOPSPartnerManager.GetByIdsAsync called with IDs: [{Ids}]", string.Join(", ", ids));

        // ==========================================
        // OPTIMIZATION: Use _context.Partners directly for IQueryable with AsNoTracking()
        // ==========================================
        var partners = _context.Partners
            .AsNoTracking()
            .Include(p => p.PartnerGroup)
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
    /// Excludes values in the range 8000-9999 from the calculation
    /// Considers all partners regardless of deletion status to ensure unique values
    /// </summary>
    private async Task<int> GetNextErpDimValueAsync()
    {
        var highestErpDimValue = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue 
                && (p.ErpDimValue.Value < 8000 || p.ErpDimValue.Value > 9999))
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

        // Get the next ErpDimValue for this partner (only if not already assigned)
        var nextErpDimValue = entity.ErpDimValue ?? await GetNextErpDimValueAsync();

        // Now approve the partner (this sets the approval status, audit trail, and ErpDimValue if not already set)
        entity.ApprovePartner(int.Parse(userId), userName, nextErpDimValue);
        await PartnerRepository.UpdateAsync(entity);
        
        // Load relationships and return updated model
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);
        var model = await MapEntityToModelAsync(entity, _mapper, user);
        return await MapEntityToModelWithPermissionsAsync(model, user, entity);
    }

    /// <summary>
    /// Unapproves an approved partner (Admin only) - unlocks data fields and records unapproval audit trail
    /// </summary>
    public async Task<PartnerModel?> UnapprovePartnerAsync(ClaimsPrincipal user, int id, StatusChangeRequest request)
    {
        var entity = await PartnerRepository.GetByIdAsync(id, ["LiaisonOffice"]);
        if (entity == null)
            return null;

        // Check if user has admin permissions for unapproval
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!userRoles.Contains("PARTNER_GLOB_ADMIN"))
        {
            throw new UnauthorizedAccessException("Only Partnership Global Administrators can unapprove partners.");
        }

        // Get user information for audit trail
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown Admin";

        // Now unapprove the partner (this sets the approval status and audit trail)
        entity.UnapprovePartner(int.Parse(userId), userName);
        await PartnerRepository.UpdateAsync(entity);
        
        // Load relationships and return updated model
        await entity.LoadOrganizationUnitRelationshipsAsync(_context);
        var model = await MapEntityToModelAsync(entity, _mapper, user);
        return await MapEntityToModelWithPermissionsAsync(model, user, entity);
    }

    #endregion

    #region Partner Related Data Methods



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

            // ==========================================
            // OPTIMIZATION: Added AsNoTracking() for read-only query
            // ==========================================
            var partner = await _context.Partners
                .AsNoTracking()
                .Where(p => p.Name.ToLower() == name.ToLower() && !p.IsDeleted)
                .Include(p => p.PartnerGroup)
                .Include(p => p.LiaisonOffice)
                .FirstOrDefaultAsync();

            if (partner == null)
            {
                return null;
            }

            // Load organization unit relationships
            await partner.LoadOrganizationUnitRelationshipsAsync(_context);

            // Apply access control filters to ensure user has permission to access this partner
            var query = _context.Partners
                .AsNoTracking()
                .Where(p => p.Id == partner.Id)
                .Include(p => p.PartnerGroup)
                .Include(p => p.LiaisonOffice)
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
    /// Gets all interactions associated with a specific partner
    /// Used for opportunity creation from interactions
    /// </summary>
    /// <param name="partnerId">The partner ID</param>
    /// <returns>List of interaction summaries for the partner</returns>
    public async Task<IEnumerable<InteractionSummaryModel>> GetPartnerInteractionsAsync(int partnerId)
    {
        try
        {
            _logger?.LogInformation($"📋 [MANAGER] Getting interactions for partner {partnerId}");

            // ==========================================
            // OPTIMIZATION: Split queries with AsNoTracking() to avoid Cartesian product
            // ==========================================
            
            // Load interactions first (simple query)
            var interactions = await _context.Interactions
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.InteractionPartners.Any(ip => ip.PartnerId == partnerId))
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            if (!interactions.Any())
            {
                _logger?.LogInformation($"✅ [MANAGER] Found 0 interactions for partner {partnerId}");
                return new List<InteractionSummaryModel>();
            }

            var interactionIds = interactions.Select(i => i.Id).ToList();

            // Load related data in separate queries to avoid Cartesian product
            var interactionPartners = await _context.InteractionPartners
                .AsNoTracking()
                .Where(ip => interactionIds.Contains(ip.InteractionId))
                .ToListAsync();

            var interactionContacts = await _context.InteractionContacts
                .AsNoTracking()
                .Where(ic => interactionIds.Contains(ic.InteractionId))
                .Include(ic => ic.Contact)
                .ToListAsync();

            var interactionUsers = await _context.InteractionUsers
                .AsNoTracking()
                .Where(iu => interactionIds.Contains(iu.InteractionId))
                .Include(iu => iu.User)
                .ToListAsync();

            // Group by interaction ID for efficient assignment
            var partnersByInteraction = interactionPartners
                .GroupBy(ip => ip.InteractionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var contactsByInteraction = interactionContacts
                .GroupBy(ic => ic.InteractionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var usersByInteraction = interactionUsers
                .GroupBy(iu => iu.InteractionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Assign collections to interactions
            foreach (var interaction in interactions)
            {
                if (partnersByInteraction.TryGetValue(interaction.Id, out var partners))
                    interaction.InteractionPartners = partners;
                if (contactsByInteraction.TryGetValue(interaction.Id, out var contacts))
                    interaction.InteractionContacts = contacts;
                if (usersByInteraction.TryGetValue(interaction.Id, out var users))
                    interaction.InteractionUsers = users;
            }

            var summaries = interactions.Select(i => new InteractionSummaryModel
            {
                Id = i.Id,
                Subject = i.Subject ?? string.Empty,
                Description = i.Description ?? string.Empty,
                Date = i.Date,
                Type = i.Type.ToString(),
                Status = i.Status.ToString(),
                Location = i.Location ?? string.Empty,
                ContactCount = i.InteractionContacts?.Count ?? 0,
                UserCount = i.InteractionUsers?.Count ?? 0
            }).ToList();

            _logger?.LogInformation($"✅ [MANAGER] Found {summaries.Count} interactions for partner {partnerId}");

            return summaries;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"❌ [MANAGER] Error getting interactions for partner {partnerId}");
            throw;
        }
    }

    /// <summary>
    /// Performs smart search for partners using AI-powered search capabilities
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> SmartSearchPartnersAsync(
        ClaimsPrincipal user, 
        string searchText, 
        int maxResults = 50,
        PaginationRequest request = null)
    {
        try
        {
            // Perform smart search to get accessible partners
            var smartSearchResult = await PerformSmartSearchAsync<UNOPSPartner>(searchText, false, maxResults);
            var accessiblePartners = await FilterAccessiblePartners(smartSearchResult.Results.Select(r => r.Entity).ToList(), user);

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
    /// Performs smart search for partners using AI-powered search capabilities
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> PerformSmartSearchAsync(
        ClaimsPrincipal user, 
        string searchText, 
        bool includeInactive = false,
        int maxResults = 50,
        PaginationRequest request = null)
    {
        return await SmartSearchPartnersAsync(user, searchText, maxResults, request);
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
    /// OPTIMIZED: Uses AsNoTracking() for read-only query
    /// </summary>
    private async Task<List<UNOPSPartner>> FilterAccessiblePartners(List<UNOPSPartner> partners, ClaimsPrincipal user)
    {
        try
        {
            var accessiblePartners = new List<UNOPSPartner>();
            
            foreach (var partner in partners)
            {
                // ==========================================
                // OPTIMIZATION: Added AsNoTracking() for read-only query
                // ==========================================
                var query = _context.Partners
                    .AsNoTracking()
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

    /// <summary>
    /// Soft deletes OrganizationUnitRelationship records for a given entity
    /// </summary>
    private async Task SoftDeleteOrganizationUnitRelationshipsAsync(int entityId, string entityType)
    {
        var relationships = await _context.OrganizationUnitRelationships
            .Where(r => r.EntityId == entityId && r.EntityType == entityType && !r.IsDeleted)
            .ToListAsync();

        // Get current user ID from claims
        var currentUser = GetCurrentUserOrSystemContext();
        var userIdClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var id) ? id : 0;

        foreach (var relationship in relationships)
        {
            relationship.SetDeleteAuditData(userId);
        }

        await _context.SaveChangesAsync();
    }

}