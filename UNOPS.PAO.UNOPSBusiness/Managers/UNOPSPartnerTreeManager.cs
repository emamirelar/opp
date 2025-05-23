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

public class UNOPSPartnerTreeManager : IPartnerTreeManager
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

    private static ExternalPartnerTreeModel MapEntityToExternalModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartnerTree, ExternalPartnerTreeModel>(entity);

        return result;
    }

    public UNOPSPartnerTreeManager(IMapper mapper, UNOPSAppDbContext context, PartnerTreeService partnerTreeService)
    {
        this.mapper = mapper;
        this.partnerTreeService = partnerTreeService;
    }

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model)
    {
        var entity = mapper.Map<UNOPSPartnerTree>(model);
        
        var result = await partnerTreeService.CreatePartnerTreeAsync(entity);

        return await MapEntityToModel((UNOPSPartnerTree)result, mapper);
    }

    public IEnumerable<PartnerTreeModel> GetPartnerTreesAsync(int userId, string sortBy = "Name", bool ascending = true)
    {
        var allTrees = partnerTreeService.GetAllPartnerTreesAsync().Result;

        // Convert entities to models first
        var treeModels = new List<PartnerTreeModel>();
        foreach (var tree in allTrees)
        {
            treeModels.Add(MapEntityToModel((UNOPSPartnerTree)tree, mapper).Result);
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

    public async Task<PartnerTreeModel?> GetPartnerTree(int userId, int id)
    {
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(id);
        if (item == null)
        {
            return default;
        }

        return await MapEntityToModel((UNOPSPartnerTree)item, mapper);
    }

    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return partnerTreeService.GetAllPartnerTreesAsync().Result
            .Select(x => MapEntityToExternalModel((UNOPSPartnerTree)x, mapper));
    }

    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await partnerTreeService.GetPartnerTreeByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner Level {id} does not exist.");
        }

        return MapEntityToExternalModel((UNOPSPartnerTree)item, mapper);
    }

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model)
    {
        var entity = await partnerTreeService.GetPartnerTreeByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner Level {model.Id} does not exist.");
        }

        mapper.Map(model, entity);

        await partnerTreeService.UpdatePartnerTreeAsync(entity);

        return await MapEntityToModel((UNOPSPartnerTree)entity, mapper);
    }

    public async Task DeletePartnerTreeAsync(int userId, int id)
    {
        var entity = await partnerTreeService.GetPartnerTreeByIdAsync(id);

        if (entity != null)
        {
            await partnerTreeService.DeletePartnerTreeAsync(entity.Code);
        }
    }

    public IEnumerable<object> GetCategoryAndGroupStructure(int userId)
    {
        // Use existing GetPartnerTrees method which already applies MapEntityToModel
        var partnerTreeStructure = GetPartnerTreesAsync(userId).ToList();
        
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
                // Create category object - using PartnerCategoryCode/Name if they're non-null (they should be at this point)
                var categoryCode = tree.Data.PartnerCategoryCode ?? tree.Data.Code;
                var categoryName = tree.Data.PartnerCategoryName ?? tree.Data.Name;
                
                var category = new
                {
                    partnerCategoryId = tree.Data.Id,
                    partnerCategoryCode = categoryCode,
                    partnerCategoryName = categoryName,
                    children = new List<object>()
                };
                
                // Collect all editable groups under this category
                if (tree.Children != null && tree.Children.Any())
                {
                    CollectAllEditableGroups(tree.Children, (List<object>)category.children);
                }
                
                categories.Add(category);
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
                
                // Add this node as a group
                groupList.Add(new
                {
                    partnerGroupId = node.Data.Id,
                    partnerGroupCode = groupCode,
                    partnerGroupName = groupName
                });
            }
            
            // Recursively process its children regardless of their editability
            // This ensures we check all levels for editable groups
            if (node.Children != null && node.Children.Any())
            {
                CollectAllEditableGroups(node.Children, groupList);
            }
        }
    }
    
    public async Task<PartnerTreeModel?> GetPartnerTreeByCode(int userId, string code)
    {
        var item = await partnerTreeService.GetPartnerTreeByCodeAsync(code);

        if (item == null)
        {
            return default;
        }

        return await MapEntityToModel((UNOPSPartnerTree)item, mapper);
    }
    
}