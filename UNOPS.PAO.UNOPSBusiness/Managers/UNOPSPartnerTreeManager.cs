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

public class UNOPSPartnerTreeManager : IPartnerTreeManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSPartnerTree> partnerTreeRepository;
    private CommonEntityRepository commonRepository;
    
    // Special codes where Level_2 children should have editable Partner Category
    private readonly string[] specialParentCodes = { "MULTILATERAL", "GOVERNMENT" };

    //private string[] includes = ["Currency", "Documents"];

    private static PartnerTreeModel MapEntityToModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = new PartnerTreeModel
        {
            Data = mapper.Map<UNOPSPartnerTree, PartnerTreeDataModel>(entity)
        };

        // Set the editability flags for this individual model
        if (result.Data != null)
        {
            result.Data.PartnerCategoryEditable = DeterminePartnerCategoryEditable(result.Data);
            result.Data.PartnerGroupEditable = DeterminePartnerGroupEditable(result.Data);
        }

        return result;
    }
    
    // Determine if Partner Category should be editable
    private static bool DeterminePartnerCategoryEditable(PartnerTreeDataModel data)
    {
        if (data == null) return false;
        
        // Special codes array for Level_2 children with editable Partner Category
        string[] specialCodes = { "MULTILATERAL", "GOVERNMENT" };
        
        // For Level_1 partners (except MULTILATERAL and GOVERNMENT)
        if (data.Type == "Level_1" && !specialCodes.Contains(data.Code))
        {
            return true;
        }
        
        // For Level_2 children of MULTILATERAL or GOVERNMENT
        if (data.Type == "Level_2" && !string.IsNullOrEmpty(data.Parent))
        {
            return specialCodes.Contains(data.Parent);
        }
        
        return false;
    }
    
    // Determine if Partner Group should be editable
    private static bool DeterminePartnerGroupEditable(PartnerTreeDataModel data)
    {
        if (data == null || string.IsNullOrEmpty(data.Parent) || string.IsNullOrEmpty(data.Type)) 
            return false;
        
        // Special codes array for partner group editability rules
        string[] specialCodes = { "MULTILATERAL", "GOVERNMENT" };
        
        var levelParts = data.Type.Split('_');
        if (levelParts.Length < 2 || !int.TryParse(levelParts[1], out int level))
            return false;
        
        // Rule 1: Level 2 or higher - if NOT a special code parent, make partner group editable
        if (level >= 2 && !specialCodes.Contains(data.Parent))
        {
            return true;
        }
        
        // Rule 2: Level 3 or higher - if IS a special code parent, make partner group editable
        if (level >= 3 && specialCodes.Contains(data.Parent))
        {
            return true;
        }
        
        return false;
    }
    
    // Process a single item with parent and grandparent context
    private static void ProcessSingleItemWithHigherLevelRules(PartnerTreeDataModel data, string? parentCode, string? grandparentCode)
    {
        if (data == null || string.IsNullOrEmpty(data.Type))
            return;
            
        // Special codes array for parent category rules
        string[] specialCodes = { "MULTILATERAL", "GOVERNMENT" };
            
        // Parse the level
        var levelParts = data.Type.Split('_');
        if (levelParts.Length < 2 || !int.TryParse(levelParts[1], out int level))
            return;
            
        // Apply special rules for level 3 or higher
        if (level >= 3 && !string.IsNullOrEmpty(grandparentCode))
        {
            // For Level_3 or higher with a special code grandparent, partner group is editable
            if (specialCodes.Contains(grandparentCode))
            {
                data.PartnerGroupEditable = true;
            }
        }
    }

    private static ExternalPartnerTreeModel MapEntityToExternalModel(UNOPSPartnerTree entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSPartnerTree, ExternalPartnerTreeModel>(entity);

        return result;
    }

    private UNOPSPartnerTree MapModelToEntity(PartnerTreeRequest model, UNOPSPartnerTree entity)
    {
        mapper.Map(model, entity);

//        entity.Name = String.Concat(model.Name, ' ', model.Description, ' ', model.Code, ' ', model.LastName);

        return entity;
    }

    private UNOPSPartnerTree MapModelToEntity(PartnerTreeRequest model)
    {
        return MapModelToEntity(model, new UNOPSPartnerTree());
    }

    public UNOPSPartnerTreeManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        this.mapper = mapper;
        partnerTreeRepository = new BaseRepository<UNOPSPartnerTree>(context, configuration);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model)
    {
        var entity = mapper.Map<UNOPSPartnerTree>(model);
        
        await partnerTreeRepository.AddAsync(entity);

        var result = MapEntityToModel(entity, mapper);
        
        // Process children if any exist
        if (result.Children != null && result.Children.Any())
        {
            ProcessTreeData(result.Children);
        }
        
        return result;
    }

    public IEnumerable<PartnerTreeModel> GetPartnerTrees(int userId, string sortBy = "Name", bool ascending = true)
    {
        var allTrees = partnerTreeRepository
            .GetAllSortedAsync(sortBy, ascending)
            .Result
            .Select(x => MapEntityToModel(x, mapper))
            .ToList();

        var lookup = allTrees.ToLookup(x => x.Data.Parent);
        var result = BuildHierarchy(lookup, string.Empty).ToList();
        
        // Process the tree data to set editability flags
        ProcessTreeData(result);
        
        return result;
    }
    
    // Process tree data to set editability flags
    private void ProcessTreeData(IEnumerable<PartnerTreeModel> nodes)
    {
        if (nodes == null) return;
        
        // First pass: Set basic flags
        foreach (var node in nodes)
        {
            if (node.Data != null)
            {
                // Set partnerCategoryEditable based on rules
                node.Data.PartnerCategoryEditable = DeterminePartnerCategoryEditable(node.Data);
                
                // Set initial partnerGroupEditable flags
                node.Data.PartnerGroupEditable = DeterminePartnerGroupEditable(node.Data);
            }
            
            // Process children recursively
            if (node.Children != null && node.Children.Any())
            {
                ProcessTreeData(node.Children);
            }
        }
        
        // Second pass: Process Level_3 and higher items with additional rules
        var parentMap = new Dictionary<string, PartnerTreeModel>();
        PopulateParentMap(nodes, parentMap); // Make sure we have a complete parent map
        ProcessHigherLevelItems(nodes, parentMap);
    }
    
    // Helper method to populate the parent map with all nodes in the tree
    private void PopulateParentMap(IEnumerable<PartnerTreeModel> nodes, Dictionary<string, PartnerTreeModel> parentMap)
    {
        if (nodes == null) return;
        
        foreach (var node in nodes)
        {
            if (node.Data?.Code != null)
            {
                parentMap[node.Data.Code] = node;
            }
            
            // Recursively add children
            if (node.Children != null && node.Children.Any())
            {
                PopulateParentMap(node.Children, parentMap);
            }
        }
    }
    
    // Second pass to process Level_3 and Level_4 items
    private void ProcessHigherLevelItems(IEnumerable<PartnerTreeModel> nodes, Dictionary<string, PartnerTreeModel> parentMap)
    {
        if (nodes == null) return;
        
        // Then process each node to apply the rules
        foreach (var node in nodes)
        {
            ProcessNodeWithParentContext(node, parentMap);
            
            // Process children recursively
            if (node.Children != null && node.Children.Any())
            {
                ProcessHigherLevelItems(node.Children, parentMap);
            }
        }
    }
    
    // Process a single node with its parent context
    private void ProcessNodeWithParentContext(PartnerTreeModel node, Dictionary<string, PartnerTreeModel> parentMap)
    {
        if (node?.Data == null || string.IsNullOrEmpty(node.Data.Parent)) return;
        
        var levelParts = node.Data.Type?.Split('_');
        if (levelParts?.Length < 2 || !int.TryParse(levelParts[1], out int level)) return;
        
        // For Level_3 and higher, check additional rules
        if (level >= 3 && parentMap.TryGetValue(node.Data.Parent, out var parentNode) && 
            parentNode.Data != null && !string.IsNullOrEmpty(parentNode.Data.Parent))
        {
            // Get the grandparent code (parent of the parent)
            var grandparentCode = parentNode.Data.Parent;
            
            // Apply special rules
            ProcessSingleItemWithHigherLevelRules(node.Data, node.Data.Parent, grandparentCode);
        }
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
        var item = await partnerTreeRepository.GetByIdAsync(id);
        if (item == null)
        {
            return default;
        }

        var result = MapEntityToModel(item, mapper);
        
        // Process editability flags for the tree and its children
        if (result != null)
        {
            // For a single tree view, we need to apply the same rules
            // Set basic flags for the main item
            if (result.Data != null)
            {
                result.Data.PartnerCategoryEditable = DeterminePartnerCategoryEditable(result.Data);
                result.Data.PartnerGroupEditable = DeterminePartnerGroupEditable(result.Data);
            }
            
            // Process children if any exist
            if (result.Children != null && result.Children.Any())
            {
                ProcessTreeData(result.Children);
            }
        }
        
        return result;
    }

    /*public async Task<string?> GetContactStage(int id)
    {
        var item = await contactRepository.GetByIdAsync(id);

        if (item == null)
        {
            return null;
        }

        return item.Stage;
    }*/

    public IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees()
    {
        return partnerTreeRepository
            .GetAll()
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id)
    {
        var item = await partnerTreeRepository.GetByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Partner Level {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    public async Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model)
    {
        var entity = await partnerTreeRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Partner Level {model.Id} does not exist.");
        }

        mapper.Map(model, entity);

        await partnerTreeRepository.UpdateAsync(entity);

        var result = MapEntityToModel(entity, mapper);
        
        // Process children if any exist
        if (result.Children != null && result.Children.Any())
        {
            ProcessTreeData(result.Children);
        }
        
        return result;
    }

    /*public async Task<ContactModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await contactRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        entity.Stage = newStage;

        await contactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }*/

    public async Task DeletePartnerTreeAsync(int userId, int id)
    {
        var entity = await partnerTreeRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await partnerTreeRepository.Delete(entity);
        }
    }
}