namespace UNOPS.PAO.UNOPSBusiness.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using System.Security.Claims;
using System.Linq;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using Microsoft.Extensions.Configuration;

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
            
            var partnerCategory = await partnerTreeService.GetPartnerCategoryByPartnerGroupCodeAsync(data.PartnerGroupCode);
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
        
        // Add permissions using the helper method from BaseUNOPSManager
        result.Permissions = await GetEntityPermissionsAsync(entity, user);
        
        return result;
    }

    private static ExternalPartnerTreeModel MapEntityToExternalModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartnerTree, ExternalPartnerTreeModel>(entity);

        return result;
    }

    public UNOPSPartnerTreeManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService, IBusinessSecurityService securityService)
        : base(mapper, context, configuration, null, securityService)
    {
        this.mapper = mapper;
        this.partnerTreeService = partnerTreeService;
    }

    [RBAC("create", Entity = "PartnerTree")]
    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model)
    {
        // RBAC interceptor handles security enforcement
        var entity = mapper.Map<UNOPSPartnerTree>(model);
        
        var result = await partnerTreeService.CreatePartnerTreeAsync(entity);

        return await MapEntityToModelWithPermissionsAsync(result, mapper, user);
    }

    [RBAC("read", Entity = "PartnerTree", ApplyRowFiltering = true)]
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
        var lookup = treeModels.ToLookup(x => x.Data.Parent);
        
        // Return the hierarchical structure
        return BuildHierarchy(lookup, string.Empty).ToList();
    }

    private IEnumerable<PartnerTreeModel> BuildHierarchy(ILookup<string, PartnerTreeModel> lookup, string parentCode, HashSet<string> visitedCodes = null)
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

    [RBAC("read", Entity = "PartnerTree", RequireEntityAccess = true, EntityIdParameterName = "id")]
    public async Task<PartnerTreeModel?> GetPartnerTreeAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(id);
        if (item == null) return null;

        return await MapEntityToModelWithPermissionsAsync(item, mapper, user);
    }

    [SkipRBAC(Reason = "External API method - no user context")]
    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return partnerTreeService.GetAllPartnerTreesAsync().Result
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    [SkipRBAC(Reason = "External API method - no user context")]
    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner Level {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    [RBAC("update", Entity = "PartnerTree", RequireEntityAccess = true, EntityIdParameterName = "model.Id")]
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

    [RBAC("delete", Entity = "PartnerTree", RequireEntityAccess = true, EntityIdParameterName = "id")]
    public async Task DeletePartnerTreeAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var entity = await partnerTreeService.GetPartnerTreeByIdAsync(id);
        if (entity == null) return;

        await partnerTreeService.DeletePartnerTreeAsync(entity.Code);
    }

    [RBAC("read", Entity = "PartnerTree", ApplyRowFiltering = true)]
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
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
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
}