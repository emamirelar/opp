namespace UNOPS.PAO.UNOPSBusiness.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.PartnerTrees;

public class UNOPSPartnerTreeManager : BaseUNOPSManager, IPartnerTreeManager
{
    private IMapper mapper;
    private readonly PartnerTreeService partnerTreeService;

    private async Task<PartnerTreeModel> MapEntityToModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var data = mapper.Map<UNOPSPartnerTree, PartnerTreeDataModel>(entity);
        
        if (data.PartnerGroupCode != null)
        {
            var partnerGroup = await partnerTreeService.GetPartnerTreeByCodeAsync(data.PartnerGroupCode);
            if (partnerGroup != null)
            {
                data.PartnerGroupName = partnerGroup.Name;
                data.PartnerGroupId = partnerGroup.Id;
                data.PartnerGroupEditable = true;
            }
            
            var partnerCategory = data.PartnerGroupId.HasValue ? 
                await partnerTreeService.GetPartnerCategoryByPartnerGroupCodeAsync(data.PartnerGroupCode) : null;
            if (partnerCategory != null)
            {
                data.PartnerCategoryName = partnerCategory.Name;
                data.PartnerCategoryCode = partnerCategory.Code;
                data.PartnerCategoryId = partnerCategory.Id;
                data.PartnerCategoryEditable = false;
            }
        } 
        else if (data.PartnerCategoryCode != null)
        {
            var partnerCategory = await partnerTreeService.GetPartnerTreeByCodeAsync(data.PartnerCategoryCode);
            if (partnerCategory != null) 
            {
                data.PartnerCategoryName = partnerCategory.Name;
                data.PartnerCategoryId = partnerCategory.Id;
                data.PartnerCategoryEditable = true;
            }
        }
        
        var result = new PartnerTreeModel
        {
            Data = data
        };
        
        return result;
    }

    // Secure methods with ClaimsPrincipal for RBAC
    private async Task<PartnerTreeModel> MapEntityToModelWithPermissionsAsync(UNOPSPartnerTree entity, IMapper mapper, ClaimsPrincipal user)
    {
        var result = await MapEntityToModel(entity, mapper);

        return await MapEntityToModelWithPermissionsAsync(result, user); ;
    }

    private static ExternalPartnerTreeModel MapEntityToExternalModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartnerTree, ExternalPartnerTreeModel>(entity);

        return result;
    }

    public UNOPSPartnerTreeManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService, IPermissionService permissionService)
        : base(mapper, context, configuration, null, "PartnerTree", permissionService)
    {
        this.mapper = mapper;
        this.partnerTreeService = partnerTreeService;
    }

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model)
    {
        // RBAC interceptor handles security enforcement
        var entity = mapper.Map<UNOPSPartnerTree>(model);
        
        var result = await partnerTreeService.CreatePartnerTreeAsync(entity);

        return await MapEntityToModelWithPermissionsAsync(result, mapper, user);
    }

    public async Task<IEnumerable<PartnerTreeModel>> GetPartnerTreesAsync(ClaimsPrincipal user, string sortBy = "Name", bool ascending = true)
    {
        // RBAC interceptor handles security enforcement and row filtering
        var allTrees = partnerTreeService.GetAllPartnerTreesAsync().Result;

        // Convert entities to models with permissions
        var treeModels = new List<PartnerTreeModel>();
        foreach (var tree in allTrees)
        {
            // TODO : Add permission but with optimisation
            // var modelWithPermissions = await MapEntityToModelWithPermissionsAsync(tree, mapper, user);
            // treeModels.Add(modelWithPermissions);
            treeModels.Add(MapEntityToModel(tree, mapper).Result);
        }

        // Create a lookup by parent code for hierarchy building
        // Normalize null and empty string to empty string for consistent hierarchy building
        var lookup = treeModels.ToLookup(x => string.IsNullOrEmpty(x.Data.Parent) ? string.Empty : x.Data.Parent);
        
        // Return the hierarchical structure
        return BuildHierarchy(lookup, string.Empty).ToList();
    }

    private IEnumerable<PartnerTreeModel> BuildHierarchy(ILookup<string, PartnerTreeModel> lookup, string parentCode, HashSet<string>? visitedCodes = null)
    {
        visitedCodes ??= new HashSet<string>();

        foreach (var item in lookup[parentCode])
        {
            if (!visitedCodes.Contains(item.Data.Code))
            {
                visitedCodes.Add(item.Data.Code);
                item.Children = BuildHierarchy(lookup, item.Data.Code, visitedCodes).ToList();
                yield return item;
            }
        }
    }

    public async Task<PartnerTreeModel?> GetPartnerTreeAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(id);
        if (item == null) return null;

        return await MapEntityToModelWithPermissionsAsync(item, mapper, user);
    }

    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return partnerTreeService.GetAllPartnerTreesAsync().Result
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner Level {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model)
    {
        // RBAC interceptor handles security enforcement
        var entity = await partnerTreeService.GetPartnerTreeByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner Level {model.Id} does not exist.");
        }

        mapper.Map(model, entity);
        await partnerTreeService.UpdatePartnerTreeAsync(entity);

        return await MapEntityToModelWithPermissionsAsync(entity, mapper, user);
    }

    public async Task DeletePartnerTreeAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var entity = await partnerTreeService.GetPartnerTreeByIdAsync(id);
        if (entity == null) return;

        await partnerTreeService.DeletePartnerTreeAsync(entity.Code);
    }

    public async Task<IEnumerable<object>> GetCategoryAndGroupStructureAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        // Use the secure method that includes row filtering and permissions
        var partnerTreeStructure = (await GetPartnerTreesAsync(user)).ToList();
        
        // Create a list to store categories
        var categories = new List<object>();
        
        // Process all levels of the tree, not just top-level items
        ProcessAllLevelsForCategories(partnerTreeStructure, categories);
        
        return categories;
    }

    // Helper method to recursively process all tree levels for categories
    private void ProcessAllLevelsForCategories(IEnumerable<PartnerTreeModel> nodes, List<object> categories)
    {
        if (nodes == null) return;
        
        foreach (var tree in nodes)
        {
            if (tree.Data == null) continue;
            
            // Check if this node is a category (has PartnerCategoryEditable == true)
            if (tree.Data.PartnerCategoryEditable)
            {
                // Check if a category with the same partnerCategoryId already exists
                var existingCategory = categories.FirstOrDefault(c => 
                {
                    var categoryObj = c as dynamic;
                    return categoryObj?.partnerCategoryId == tree.Data.PartnerCategoryId;
                });

                List<object> childrenList;
                
                if (existingCategory != null)
                {
                    // Use existing category's children list
                    childrenList = (List<object>)((dynamic)existingCategory).children;
                }
                else
                {
                    // Create new category
                    var category = new
                    {
                        partnerCategoryId = tree.Data.PartnerCategoryId,
                        partnerCategoryCode = tree.Data.PartnerCategoryCode,
                        partnerCategoryName = tree.Data.PartnerCategoryName,
                        children = new List<object>()
                    };
                    
                    childrenList = (List<object>)category.children;
                    categories.Add(category);
                }
                
                // Collect all editable groups under this category
                if (tree.Children != null && tree.Children.Any())
                {
                    CollectAllEditableGroups(tree.Children, childrenList);
                }
            }
            
            // Continue checking children nodes for more categories
            if (tree.Children != null && tree.Children.Any())
            {
                ProcessAllLevelsForCategories(tree.Children, categories);
            }
        }
    }

    // Helper method to recursively collect all editable groups under a category
    private void CollectAllEditableGroups(IEnumerable<PartnerTreeModel> nodes, List<object> groupList)
    {
        foreach (var node in nodes)
        {
            if (node.Data == null) continue;
            // Only include groups that are editable
            if (node.Data.PartnerGroupEditable)
            {
                // Use PartnerGroupCode/Name if they're non-null (they should be at this point)
                var groupCode = node.Data.PartnerGroupCode ?? node.Data.Code;
                var groupName = node.Data.PartnerGroupName ?? node.Data.Name;
                
                // Check if a group with the same partnerGroupId or partnerGroupCode already exists
                var existingGroup = groupList.FirstOrDefault(g => 
                {
                    var groupObj = g as dynamic;
                    return (groupObj?.partnerGroupId == node.Data.Id) || 
                           (groupObj?.partnerGroupCode == groupCode);
                });

                // Only add if it doesn't already exist
                if (existingGroup == null)
                {
                    // Add this node as a group
                    groupList.Add(new
                    {
                        partnerGroupId = node.Data.Id,
                        partnerGroupCode = groupCode,
                        partnerGroupName = groupName
                    });
                }
            }
            
            // Recursively process its children regardless of their editability
            // This ensures we check all levels for editable groups
            if (node.Children != null && node.Children.Any())
            {
                CollectAllEditableGroups(node.Children, groupList);
            }
        }
    }

    /// <summary>
    /// Gets basic entity data for AI prompts and generic operations
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal? user = null)
    {
        if (user != null)
        {
            return await GetPartnerTreeAsync(user, entityId);
        }
        
        // Fallback for cases without user context
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(entityId);
        if (item == null) return null;
        
        return await MapEntityToModel(item, mapper);
    }
    
    // Legacy method without user context - keeping for backward compatibility
    public async Task<PartnerTreeModel?> GetPartnerTreeByCode(int userId, string code)
    {
        var item = await partnerTreeService.GetPartnerTreeByCodeAsync(code);
        if (item == null)
        {
            return default;
        }

        return await MapEntityToModel(item, mapper);
    }

    /// <summary>
    /// Gets partner category details with related partners and their recent interactions for AI analysis
    /// </summary>
    public async Task<object> GetBasicPartnerCategoryDetailsAsync(ClaimsPrincipal user, int entityId)
    {
        // ==========================================
        // QUERY 1: Main entity - Partner Category
        // ==========================================
        var partnerCategory = await _context.PartnerTrees
            .AsNoTracking() // ✅ Read-only query optimization
            .FirstOrDefaultAsync(pt => pt.Id == entityId && !pt.IsDeleted);
        
        if (partnerCategory == null)
        {
            return new { Error = "Partner category not found" };
        }

        // Get all PartnerGroups that are descendants of this PartnerCategory (recursive)
        var partnerGroupIds = await partnerTreeService.GetAllDescendantsAsync(partnerCategory.Code);

        // ==========================================
        // QUERY 2: Partners with simple navigation properties
        // ==========================================
        var partners = await _context.Partners
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(p => partnerGroupIds.Contains(p.PartnerGroupId.Value) && !p.IsDeleted)
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .ToListAsync();

        var partnerIds = partners.Select(p => p.Id).ToList();

        // ==========================================
        // QUERY 3: Interactions (main entity only) - SPLIT QUERY OPTIMIZATION
        // Eliminates Cartesian product from multiple ThenInclude() chains
        // ==========================================
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentInteractions = await _context.Interactions
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(i => i.InteractionPartners.Any(ip => partnerIds.Contains(ip.PartnerId)) && 
                       !i.IsDeleted && 
                       i.Date >= thirtyDaysAgo)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        var recentInteractionIds = recentInteractions.Select(i => i.Id).ToList();

        // ==========================================
        // QUERY 4: InteractionPartners collection - SPLIT QUERY OPTIMIZATION
        // Load separately to avoid Cartesian product
        // ==========================================
        var interactionPartners = await _context.Set<Domain.Entities.InteractionPartner>()
            .AsNoTracking()
            .Where(ip => recentInteractionIds.Contains(ip.InteractionId))
            .Include(ip => ip.Partner)
            .ToListAsync();

        // ==========================================
        // QUERY 5: InteractionContacts collection - SPLIT QUERY OPTIMIZATION
        // Load separately to avoid Cartesian product
        // ==========================================
        var interactionContacts = await _context.Set<Domain.Entities.InteractionContact>()
            .AsNoTracking()
            .Where(ic => recentInteractionIds.Contains(ic.InteractionId))
            .Include(ic => ic.Contact)
            .ToListAsync();

        // ==========================================
        // QUERY 6: InteractionUsers collection - SPLIT QUERY OPTIMIZATION
        // Load separately to avoid Cartesian product
        // ==========================================
        var interactionUsers = await _context.Set<Domain.Entities.InteractionUser>()
            .AsNoTracking()
            .Where(iu => recentInteractionIds.Contains(iu.InteractionId))
            .Include(iu => iu.User)
                .ThenInclude(u => u.UserProfile)
            .ToListAsync();

        // Assign collections back to interactions for processing
        var interactionPartnersLookup = interactionPartners.GroupBy(ip => ip.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var interactionContactsLookup = interactionContacts.GroupBy(ic => ic.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var interactionUsersLookup = interactionUsers.GroupBy(iu => iu.InteractionId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var interaction in recentInteractions)
        {
            interaction.InteractionPartners = interactionPartnersLookup.TryGetValue(interaction.Id, out var partners_) 
                ? partners_ : new List<Domain.Entities.InteractionPartner>();
            interaction.InteractionContacts = interactionContactsLookup.TryGetValue(interaction.Id, out var contacts) 
                ? contacts : new List<Domain.Entities.InteractionContact>();
            interaction.InteractionUsers = interactionUsersLookup.TryGetValue(interaction.Id, out var users) 
                ? users : new List<Domain.Entities.InteractionUser>();
        }

        // ==========================================
        // QUERY 7: All interactions for statistics - SPLIT QUERY OPTIMIZATION
        // ==========================================
        var allInteractions = await _context.Interactions
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(i => i.InteractionPartners.Any(ip => partnerIds.Contains(ip.PartnerId)) && !i.IsDeleted)
            .ToListAsync();

        var allInteractionIds = allInteractions.Select(i => i.Id).ToList();
        
        var allInteractionPartners = await _context.Set<Domain.Entities.InteractionPartner>()
            .AsNoTracking()
            .Where(ip => allInteractionIds.Contains(ip.InteractionId))
            .Include(ip => ip.Partner)
            .ToListAsync();

        var allInteractionPartnersLookup = allInteractionPartners.GroupBy(ip => ip.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var interaction in allInteractions)
        {
            interaction.InteractionPartners = allInteractionPartnersLookup.TryGetValue(interaction.Id, out var partners_) 
                ? partners_ : new List<Domain.Entities.InteractionPartner>();
        }

        // Get unique org unit codes from interaction users
        var orgUnitCodes = interactionUsers
            .Select(iu => iu.User?.UserProfile?.OrgUnit)
            .Where(code => !string.IsNullOrEmpty(code))
            .Distinct()
            .ToList();

        // ==========================================
        // QUERY 8: Organization hierarchy lookup
        // ==========================================
        var orgUnitLookup = await _context.OrganizationHierarchies
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(oh => orgUnitCodes.Contains(oh.Code) && oh.Type == OrganizationUnitType.OrgUnit)
            .GroupBy(oh => oh.Code)
            .ToDictionaryAsync(g => g.Key, g => g.First().Name);

        // Create structured JSON for AI prompt placeholders
        var result = new
        {
            id = partnerCategory.Id,
            categoryName = partnerCategory.Description,
            categoryCode = partnerCategory.Code,
            categoryType = partnerCategory.Type?.ToString(),

            // Partner information
            partners = partners.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                status = p.Status.ToString(),
                partnerGroup = p.PartnerGroup?.Name,
                liaisonOffice = p.LiaisonOffice?.Name,
                createdDate = p.CreatedDate.ToString("yyyy-MM-dd")
            }).Cast<dynamic>().ToList(),

            // Recent interactions (last 30 days) with full details
            recentInteractions = recentInteractions.Select(i => new
            {
                id = i.Id,
                subject = i.Subject,
                description = i.Description,
                date = i.Date.ToString("yyyy-MM-dd"),
                time = i.Date.ToString("HH:mm"),
                type = i.Type.ToString(),
                location = i.Location,
                partners = i.InteractionPartners?.Select(ip => new
                {
                    id = ip.Partner.Id,
                    name = ip.Partner.Name
                }).ToList(),
                contacts = i.InteractionContacts?.Select(ic => new
                {
                    id = ic.Contact.Id,
                    name = $"{ic.Contact.FirstName} {ic.Contact.LastName}".Trim(),
                    title = ic.Contact.Title,
                    email = ic.Contact.Email
                }).ToList(),
                users = i.InteractionUsers?.Select(iu => new
                {
                    id = iu.User.Id,
                    name = iu.User.Name,
                    title = iu.User.UserProfile?.Position,
                    orgUnitCode = iu.User.UserProfile?.OrgUnit,
                    orgUnitName = !string.IsNullOrEmpty(iu.User.UserProfile?.OrgUnit) && orgUnitLookup.ContainsKey(iu.User.UserProfile.OrgUnit) 
                        ? orgUnitLookup[iu.User.UserProfile.OrgUnit] 
                        : iu.User.UserProfile?.OrgUnit
                }).ToList()
            }).Cast<dynamic>().ToList(),

            // Partner statistics
            partnerCount = partners.Count,
            activePartners = partners.Count(p => p.Status == (Domain.Entities.EntityStatus)1),
            partnerNames = string.Join(", ", partners.Select(p => p.Name)),

            // Interaction statistics
            summary = new
            {
                totalInteractions = allInteractions.Count,
                recentInteractions = recentInteractions.Count,
                lastInteractionDate = allInteractions.OrderByDescending(i => i.Date).FirstOrDefault()?.Date.ToString("yyyy-MM-dd"),
                mostActivePartners = recentInteractions
                    .SelectMany(i => i.InteractionPartners ?? new List<Domain.Entities.InteractionPartner>())
                    .GroupBy(ip => ip.Partner.Name)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList(),
                commonInteractionTypes = recentInteractions
                    .GroupBy(i => i.Type.ToString())
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList()
            },

            // Audit information
            auditInfo = new
            {
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = partnerCategory.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not available" ?? "Not modified"
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

        return result;
    }

    /// <summary>
    /// Gets partner group details with related partners and their recent interactions for AI analysis
    /// </summary>
    public async Task<object> GetBasicPartnerGroupDetailsAsync(ClaimsPrincipal user, int entityId)
    {
        // ==========================================
        // QUERY 1: Main entity - Partner Group
        // ==========================================
        var partnerGroup = await _context.PartnerTrees
            .AsNoTracking() // ✅ Read-only query optimization
            .FirstOrDefaultAsync(pt => pt.Id == entityId && !pt.IsDeleted);
        
        if (partnerGroup == null)
        {
            return new { Error = "Partner group not found" };
        }

        // ==========================================
        // QUERY 2: Partners with simple navigation properties
        // ==========================================
        var partners = await _context.Partners
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(p => p.PartnerGroupId == entityId && !p.IsDeleted)
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .ToListAsync();

        var partnerIds = partners.Select(p => p.Id).ToList();

        // ==========================================
        // QUERY 3: Interactions (main entity only) - SPLIT QUERY OPTIMIZATION
        // Eliminates Cartesian product from multiple ThenInclude() chains
        // ==========================================
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentInteractions = await _context.Interactions
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(i => i.InteractionPartners.Any(ip => partnerIds.Contains(ip.PartnerId)) && 
                       !i.IsDeleted && 
                       i.Date >= thirtyDaysAgo)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        var recentInteractionIds = recentInteractions.Select(i => i.Id).ToList();

        // ==========================================
        // QUERY 4: InteractionPartners collection - SPLIT QUERY OPTIMIZATION
        // Load separately to avoid Cartesian product
        // ==========================================
        var interactionPartners = await _context.Set<Domain.Entities.InteractionPartner>()
            .AsNoTracking()
            .Where(ip => recentInteractionIds.Contains(ip.InteractionId))
            .Include(ip => ip.Partner)
            .ToListAsync();

        // ==========================================
        // QUERY 5: InteractionContacts collection - SPLIT QUERY OPTIMIZATION
        // Load separately to avoid Cartesian product
        // ==========================================
        var interactionContacts = await _context.Set<Domain.Entities.InteractionContact>()
            .AsNoTracking()
            .Where(ic => recentInteractionIds.Contains(ic.InteractionId))
            .Include(ic => ic.Contact)
            .ToListAsync();

        // ==========================================
        // QUERY 6: InteractionUsers collection - SPLIT QUERY OPTIMIZATION
        // Load separately to avoid Cartesian product
        // ==========================================
        var interactionUsers = await _context.Set<Domain.Entities.InteractionUser>()
            .AsNoTracking()
            .Where(iu => recentInteractionIds.Contains(iu.InteractionId))
            .Include(iu => iu.User)
                .ThenInclude(u => u.UserProfile)
            .ToListAsync();

        // Assign collections back to interactions for processing
        var interactionPartnersLookup = interactionPartners.GroupBy(ip => ip.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var interactionContactsLookup = interactionContacts.GroupBy(ic => ic.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var interactionUsersLookup = interactionUsers.GroupBy(iu => iu.InteractionId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var interaction in recentInteractions)
        {
            interaction.InteractionPartners = interactionPartnersLookup.TryGetValue(interaction.Id, out var partners_) 
                ? partners_ : new List<Domain.Entities.InteractionPartner>();
            interaction.InteractionContacts = interactionContactsLookup.TryGetValue(interaction.Id, out var contacts) 
                ? contacts : new List<Domain.Entities.InteractionContact>();
            interaction.InteractionUsers = interactionUsersLookup.TryGetValue(interaction.Id, out var users) 
                ? users : new List<Domain.Entities.InteractionUser>();
        }

        // ==========================================
        // QUERY 7: All interactions for statistics - SPLIT QUERY OPTIMIZATION
        // ==========================================
        var allInteractions = await _context.Interactions
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(i => i.InteractionPartners.Any(ip => partnerIds.Contains(ip.PartnerId)) && !i.IsDeleted)
            .ToListAsync();

        var allInteractionIds = allInteractions.Select(i => i.Id).ToList();
        
        var allInteractionPartners = await _context.Set<Domain.Entities.InteractionPartner>()
            .AsNoTracking()
            .Where(ip => allInteractionIds.Contains(ip.InteractionId))
            .Include(ip => ip.Partner)
            .ToListAsync();

        var allInteractionPartnersLookup = allInteractionPartners.GroupBy(ip => ip.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var interaction in allInteractions)
        {
            interaction.InteractionPartners = allInteractionPartnersLookup.TryGetValue(interaction.Id, out var partners_) 
                ? partners_ : new List<Domain.Entities.InteractionPartner>();
        }

        // Get unique org unit codes from interaction users
        var orgUnitCodes = interactionUsers
            .Select(iu => iu.User?.UserProfile?.OrgUnit)
            .Where(code => !string.IsNullOrEmpty(code))
            .Distinct()
            .ToList();

        // ==========================================
        // QUERY 8: Organization hierarchy lookup
        // ==========================================
        var orgUnitLookup = await _context.OrganizationHierarchies
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(oh => orgUnitCodes.Contains(oh.Code) && oh.Type == OrganizationUnitType.OrgUnit)
            .GroupBy(oh => oh.Code)
            .ToDictionaryAsync(g => g.Key, g => g.First().Name);

        // Create structured JSON for AI prompt placeholders
        var result = new
        {
            id = partnerGroup.Id,
            groupName = partnerGroup.Name,
            groupCode = partnerGroup.Code,
            groupType = partnerGroup.Type?.ToString(),

            // Partner information
            partners = partners.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                status = p.Status.ToString(),
                liaisonOffice = p.LiaisonOffice?.Name,
                website = p.Name,
                description = p.Name,
                createdDate = p.CreatedDate.ToString("yyyy-MM-dd")
            }).Cast<dynamic>().ToList(),

            // Recent interactions (last 30 days) with full details
            recentInteractions = recentInteractions.Select(i => new
            {
                id = i.Id,
                subject = i.Subject,
                description = i.Description,
                date = i.Date.ToString("yyyy-MM-dd"),
                time = i.Date.ToString("HH:mm"),
                type = i.Type.ToString(),
                location = i.Location,
                partners = i.InteractionPartners?.Select(ip => new
                {
                    id = ip.Partner.Id,
                    name = ip.Partner.Name
                }).ToList(),
                contacts = i.InteractionContacts?.Select(ic => new
                {
                    id = ic.Contact.Id,
                    name = $"{ic.Contact.FirstName} {ic.Contact.LastName}".Trim(),
                    title = ic.Contact.Title,
                    email = ic.Contact.Email
                }).ToList(),
                users = i.InteractionUsers?.Select(iu => new
                {
                    id = iu.User.Id,
                    name = iu.User.Name,
                    title = iu.User.UserProfile?.Position,
                    orgUnitCode = iu.User.UserProfile?.OrgUnit,
                    orgUnitName = !string.IsNullOrEmpty(iu.User.UserProfile?.OrgUnit) && orgUnitLookup.ContainsKey(iu.User.UserProfile.OrgUnit) 
                        ? orgUnitLookup[iu.User.UserProfile.OrgUnit] 
                        : iu.User.UserProfile?.OrgUnit
                }).ToList()
            }).Cast<dynamic>().ToList(),

            // Partner statistics
            partnerCount = partners.Count,
            activePartners = partners.Count(p => p.Status == (Domain.Entities.EntityStatus)1),
            partnerNames = string.Join(", ", partners.Select(p => p.Name)),

            // Interaction statistics
            summary = new
            {
                totalInteractions = allInteractions.Count,
                recentInteractions = recentInteractions.Count,
                lastInteractionDate = allInteractions.OrderByDescending(i => i.Date).FirstOrDefault()?.Date.ToString("yyyy-MM-dd"),
                mostActivePartners = recentInteractions
                    .SelectMany(i => i.InteractionPartners ?? new List<Domain.Entities.InteractionPartner>())
                    .GroupBy(ip => ip.Partner.Name)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList(),
                commonInteractionTypes = recentInteractions
                    .GroupBy(i => i.Type.ToString())
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList()
            },

            // Audit information
            auditInfo = new
            {
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = partnerGroup.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not available" ?? "Not modified"
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

        return result;
    }

    /// <summary>
    /// Gets partner category with related partners for news analysis (simplified model without interactions)
    /// </summary>
    public async Task<object> GetPartnerCategoryNewsDetailsAsync(ClaimsPrincipal user, int entityId)
    {
        // Get the partner category (PartnerTree) details
        var partnerCategory = await _context.PartnerTrees
            .AsNoTracking() // ✅ Read-only query optimization
            .FirstOrDefaultAsync(pt => pt.Id == entityId && !pt.IsDeleted);
        
        if (partnerCategory == null)
        {
            return new { Error = "Partner category not found" };
        }

        // Get all PartnerGroups that are descendants of this PartnerCategory (recursive)
        var partnerGroupIds = await partnerTreeService.GetAllDescendantsAsync(partnerCategory.Code);

        // Get Partners that belong to these PartnerGroups with additional details
        var partners = await _context.Partners
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(p => partnerGroupIds.Contains(p.PartnerGroupId.Value) && !p.IsDeleted)
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .ToListAsync();

        // Create structured JSON for AI prompt placeholders
        var result = new
        {
            id = partnerCategory.Id,
            categoryName = partnerCategory.Description,
            categoryCode = partnerCategory.Code,
            categoryType = partnerCategory.Type?.ToString(),

            // Partner information for news search
            partners = partners.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                status = p.Status.ToString(),
                partnerGroup = p.PartnerGroup?.Name,
                liaisonOffice = p.LiaisonOffice?.Name,
                website = p.Name,
                description = p.Name
            }).Cast<dynamic>().ToList(),

            // Partner names for search queries
            partnerNames = string.Join(", ", partners.Select(p => p.Name)),
            partnerCount = partners.Count,

            // Search context
            searchContext = new
            {
                focusAreas = new[]
                {
                    "Development funding and partnerships",
                    "Policy changes affecting international cooperation",
                    "New initiatives or programs",
                    "Strategic partnerships",
                    "Regional development activities"
                },
                newsSources = new[]
                {
                    "Google News",
                    "Devex",
                    "Donor Tracker",
                    "Development news outlets"
                },
                timeframe = "Recent news stories (last 30 days)",
                relevanceContext = "UNOPS operations and partnerships"
            },

            // Statistics
            summary = new
            {
                totalPartners = partners.Count,
                activePartners = partners.Count(p => p.Status == (Domain.Entities.EntityStatus)1),
                partnersWithWebsites = partners.Count(p => !string.IsNullOrEmpty(p.Name))
            },

            // Audit information
            auditInfo = new
            {
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = partnerCategory.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not available" ?? "Not modified"
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

        return result;
    }

    /// <summary>
    /// Gets partner group with related partners for news analysis (simplified model without interactions)
    /// </summary>
    public async Task<object> GetPartnerGroupNewsDetailsAsync(ClaimsPrincipal user, int entityId)
    {
        // Get the partner group (PartnerTree) details
        var partnerGroup = await _context.PartnerTrees
            .AsNoTracking() // ✅ Read-only query optimization
            .FirstOrDefaultAsync(pt => pt.Id == entityId && !pt.IsDeleted);
        
        if (partnerGroup == null)
        {
            return new { Error = "Partner group not found" };
        }

        // Get Partners that belong to this PartnerGroup with additional details
        var partners = await _context.Partners
            .AsNoTracking() // ✅ Read-only query optimization
            .Where(p => p.PartnerGroupId == entityId && !p.IsDeleted)
            .Include(p => p.PartnerGroup)
            .Include(p => p.LiaisonOffice)
            .ToListAsync();

        // Create structured JSON for AI prompt placeholders
        var result = new
        {
            id = partnerGroup.Id,
            groupName = partnerGroup.Name,
            groupCode = partnerGroup.Code,
            groupType = partnerGroup.Type?.ToString(),

            // Partner information for news search
            partners = partners.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                status = p.Status.ToString(),
                partnerGroup = p.PartnerGroup?.Name,
                liaisonOffice = p.LiaisonOffice?.Name,
                website = p.Name,
                description = p.Name
            }).Cast<dynamic>().ToList(),

            // Partner names for search queries
            partnerNames = string.Join(", ", partners.Select(p => p.Name)),
            partnerCount = partners.Count,

            // User context information
            orgUnit = "UNOPS",
            userOffice = "UNOPS", 
            userTitle = "Staff",
            userName = user.Identity?.Name,

            // Search context
            searchContext = new
            {
                focusAreas = new[]
                {
                    "Development funding and partnerships",
                    "Policy changes affecting international cooperation",
                    "New initiatives or programs",
                    "Strategic partnerships",
                    "Regional development activities"
                },
                newsSources = new[]
                {
                    "Google News",
                    "Devex",
                    "Donor Tracker",
                    "Development news outlets"
                },
                timeframe = "Recent news stories (last 30 days)",
                relevanceContext = "UNOPS operations and partnerships"
            },

            // Statistics
            summary = new
            {
                totalPartners = partners.Count,
                activePartners = partners.Count(p => p.Status == (Domain.Entities.EntityStatus)1),
                partnersWithWebsites = partners.Count(p => !string.IsNullOrEmpty(p.Name))
            },

            // Audit information
            auditInfo = new
            {
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = partnerGroup.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not available" ?? "Not modified"
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

        return result;
    }
}