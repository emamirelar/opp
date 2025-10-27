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
using System.Security.Claims;
using UNOPS.PAO.Models.PartnerTrees;

public class PartnerTreeManager : IPartnerTreeManager
{
    private IMapper mapper;

    private DataRepository<PartnerTree> PartnerTreeRepository;

    public PartnerTreeManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.PartnerTreeRepository = new DataRepository<PartnerTree>(context);
    }

    private PartnerTreeModel MapEntityToModel(PartnerTree entity)
    {
        var result = new PartnerTreeModel
        {
            Data = mapper.Map<PartnerTree, PartnerTreeDataModel>(entity)
        };

        return result;
    }

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model)
    {
        var entity = mapper.Map<PartnerTree>(model);

        await PartnerTreeRepository.AddAsync(entity);

        return MapEntityToModel(entity);
    }

    public IEnumerable<PartnerTreeModel> GetPartnerTreesAsync(int userId, string sortBy = "Name", bool ascending = true)
    {
        var allTrees = PartnerTreeRepository
            .GetAllSortedAsync(sortBy, ascending)
            .Result
            .Select(x => MapEntityToModel(x))
            .ToList();

        // Normalize null and empty string to empty string for consistent hierarchy building
        var lookup = allTrees.ToLookup(x => string.IsNullOrEmpty(x.Data.Parent) ? string.Empty : x.Data.Parent);
        return BuildHierarchy(lookup, string.Empty, new HashSet<string>());
    }

    private IEnumerable<PartnerTreeModel> BuildHierarchy(ILookup<string, PartnerTreeModel> lookup, string parentCode, HashSet<string> visitedCodes)
    {
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
        var item = await PartnerTreeRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<PartnerTreeModel>(item);
    }

    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return PartnerTreeRepository
            .GetAll()
            .Select(mapper.Map<ExternalPartnerTreeModel>);
    }

    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await PartnerTreeRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalPartnerTreeModel>(item);
    }

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model)
    {
        var entity = await PartnerTreeRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map(model, entity);

        await PartnerTreeRepository.UpdateAsync(entity);

        return MapEntityToModel(entity);
    }

    public async Task DeletePartnerTreeAsync(int userId, int id)
    {
        var entity = await PartnerTreeRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await PartnerTreeRepository.Delete(entity);
        }
    }

    public IEnumerable<object> GetCategoryAndGroupStructure(int userId)
    {
        // Get all partner trees
        var partnerTreeStructure = GetPartnerTreesAsync(userId).ToList();
        
        // Create a list to store categories
        var categories = new List<object>();
        
        // Process top-level items as categories
        foreach (var tree in partnerTreeStructure)
        {
            if (tree.Data == null) continue;
            
            // Create category object
            var category = new
            {
                id = tree.Data.Id,
                partnerCategoryCode = tree.Data.Code,
                partnerCategoryName = tree.Data.Name,
                children = new List<object>()
            };
            
            // Collect all groups (children) under this category
            if (tree.Children != null && tree.Children.Any())
            {
                CollectGroups(tree.Children, (List<object>)category.children);
            }
            
            categories.Add(category);
        }
        
        return categories;
    }
    
    // Helper method to recursively collect all groups under a category
    private void CollectGroups(IEnumerable<PartnerTreeModel> nodes, List<object> groupList)
    {
        foreach (var node in nodes)
        {
            if (node.Data == null) continue;
            
            // Add this node as a group
            groupList.Add(new
            {
                id = node.Data.Id,
                partnerGroupCode = node.Data.Code,
                partnerGroupName = node.Data.Name
            });
            
            // Recursively process its children
            if (node.Children != null && node.Children.Any())
            {
                CollectGroups(node.Children, groupList);
            }
        }
    }

    // Secure methods with ClaimsPrincipal - implemented in UNOPSPartnerTreeManager
    public Task<PartnerTreeModel> CreatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model)
    {
        throw new NotImplementedException("Use UNOPSPartnerTreeManager for UNOPS-specific implementation");
    }

    public Task<IEnumerable<PartnerTreeModel>> GetPartnerTreesAsync(ClaimsPrincipal user, string sortBy = "Name", bool ascending = true)
    {
        throw new NotImplementedException("Use UNOPSPartnerTreeManager for UNOPS-specific implementation");
    }

    public Task<PartnerTreeModel?> GetPartnerTreeAsync(ClaimsPrincipal user, int id)
    {
        throw new NotImplementedException("Use UNOPSPartnerTreeManager for UNOPS-specific implementation");
    }

    public Task<PartnerTreeModel?> UpdatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model)
    {
        throw new NotImplementedException("Use UNOPSPartnerTreeManager for UNOPS-specific implementation");
    }

    public Task DeletePartnerTreeAsync(ClaimsPrincipal user, int id)
    {
        throw new NotImplementedException("Use UNOPSPartnerTreeManager for UNOPS-specific implementation");
    }

    public Task<IEnumerable<object>> GetCategoryAndGroupStructureAsync(ClaimsPrincipal user)
    {
        throw new NotImplementedException("Use UNOPSPartnerTreeManager for UNOPS-specific implementation");
    }
}