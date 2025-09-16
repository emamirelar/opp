using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDomain.Entities;


namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PartnerTreeService
    {
        private readonly DataRepository<UNOPSPartnerTree> _partnerTreeRepository;
        private readonly IMemoryCache _memoryCache;
        private const string CACHE_KEY = "PARTNER_TREE_CACHE";
        private const string LEVEL_1 = "Level_1";
        private const string LEVEL_2 = "Level_2";
        

        public PartnerTreeService(DataRepository<UNOPSPartnerTree> partnerTreeRepository, IMemoryCache memoryCache)
        {
            _partnerTreeRepository = partnerTreeRepository;
            _memoryCache = memoryCache;
        }

        private async Task<IEnumerable<UNOPSPartnerTree>> LoadPartnerTreesAsync()
        {
            if (!_memoryCache.TryGetValue(CACHE_KEY, out IEnumerable<UNOPSPartnerTree>? partnerTrees))
            {
                var allPartnerTrees = await _partnerTreeRepository.GetAllSortedAsync("Type");
                partnerTrees = allPartnerTrees.Where(pt => !pt.IsDeleted);
                
                foreach (var partnerTree in partnerTrees)
                {
                    partnerTree.PartnerCategoryCode = CanModifyPartnerCategoryCodeAsync(partnerTree) ? 
                        (string.IsNullOrEmpty(partnerTree.PartnerCategoryCode) ? partnerTree.Code : partnerTree.PartnerCategoryCode) : 
                        null;
                    if (partnerTree.PartnerCategoryCode == null)
                    {
                        partnerTree.PartnerGroupCode = CanModifyPartnerGroupCodeAsync(partnerTree, partnerTrees.ToList()) ? 
                            (string.IsNullOrEmpty(partnerTree.PartnerGroupCode) ? partnerTree.Code : partnerTree.PartnerGroupCode) 
                            : null;
                    }
                }
                
                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(2));

                _memoryCache.Set(CACHE_KEY, partnerTrees, cacheOptions);

            }

            return partnerTrees ?? new List<UNOPSPartnerTree>();
        }

        public async Task<IEnumerable<UNOPSPartnerTree>> GetAllPartnerTreesAsync()
        {
            return await LoadPartnerTreesAsync();
        }

        public async Task<UNOPSPartnerTree?> GetPartnerTreeByCodeAsync(string code)
        {
            var allPartnerTrees = await LoadPartnerTreesAsync();
            return allPartnerTrees.FirstOrDefault(pt => pt.Code == code && !pt.IsDeleted);
        }

        public async Task<UNOPSPartnerTree?> GetPartnerCategoryByPartnerGroupCodeAsync(string code)
        {
            var partnerTrees = await LoadPartnerTreesAsync();
            var partnerTree = await GetPartnerTreeByCodeAsync(code);
            if (partnerTree == null)
            {
                return null;
            }
            return await GetParentCategory(partnerTree, partnerTrees.ToList());
        }

        private async Task<UNOPSPartnerTree?> GetParentCategory(UNOPSPartnerTree partnerTree, List<UNOPSPartnerTree> partnerTrees)
        {
            if (partnerTree == null)
            {
                return null;
            }
            var parent = await GetPartnerTreeByCodeAsync(partnerTree.Parent);
            if (parent == null)
            {
                return null;
            }

            if (parent.PartnerCategoryCode == null)
            {
                return await GetParentCategory(parent, partnerTrees);
            }
            
            return parent;
        }

        public async Task<UNOPSPartnerTree?> GetPartnerTreeByIdAsync(int id)
        {
            var allPartnerTrees = await LoadPartnerTreesAsync();
            return allPartnerTrees.FirstOrDefault(pt => pt.Id == id && !pt.IsDeleted);
        }

        public async Task<bool> UpdatePartnerTreeAsync(UNOPSPartnerTree partnerTree)
        {
            if (partnerTree == null) throw new ArgumentNullException(nameof(partnerTree));
            
            // Find the existing partner tree (including deleted ones for update operations)
            var existingPartnerTree = await _partnerTreeRepository.GetByIdAsync(partnerTree.Id);
            if (existingPartnerTree == null || existingPartnerTree.IsDeleted) return false;

            // Check if we should update PartnerCategoryCode based on rules
            // TODO: should throw if not allowed
            if (CanModifyPartnerCategoryCodeAsync(existingPartnerTree))
            {
                existingPartnerTree.PartnerCategoryCode = partnerTree.PartnerCategoryCode;
            }
            
            // Check if we should update PartnerGroupCode based on rules
            // TODO: should throw if not allowed
            if (CanModifyPartnerGroupCodeAsync(existingPartnerTree, (await LoadPartnerTreesAsync()).ToList()))
            {
                existingPartnerTree.PartnerGroupCode = partnerTree.PartnerGroupCode;
            }
            
            // Update other properties
            existingPartnerTree.Description = partnerTree.Description;
            existingPartnerTree.Type = partnerTree.Type;
            existingPartnerTree.Parent = string.IsNullOrWhiteSpace(partnerTree.Parent) ? "" : partnerTree.Parent;
            
            // Save changes
            await _partnerTreeRepository.UpdateAsync(existingPartnerTree);
            
            // Invalidate cache
            _memoryCache.Remove(CACHE_KEY);
            
            return true;
        }

        public async Task<UNOPSPartnerTree?> CreatePartnerTreeAsync(UNOPSPartnerTree partnerTree)
        {
            if (partnerTree == null) throw new ArgumentNullException(nameof(partnerTree));

            // Check code uniqueness (only among non-deleted partner trees)
            var existingPartnerTree = await GetPartnerTreeByCodeAsync(partnerTree.Code);
            if (existingPartnerTree != null)
            {
                throw new BusinessException($"A PartnerTree with code '{partnerTree.Code}' already exists.");
            }

            var newPartnerTree = new UNOPSPartnerTree
            {
                Name = partnerTree.Name,
                Code = partnerTree.Code,
                Status = EntityStatus.Active,
                Description = partnerTree.Description,
                Type = partnerTree.Type,
                Parent = string.IsNullOrWhiteSpace(partnerTree.Parent) ? "" : partnerTree.Parent
            };
            
            // Appliquer les règles de PartnerCategoryCode directement lors de la création
            if (CanModifyPartnerCategoryCodeAsync(newPartnerTree))
            {
                newPartnerTree.PartnerCategoryCode = partnerTree.PartnerCategoryCode;
            }
            
            // Sauvegarder d'abord pour avoir l'ID
            await _partnerTreeRepository.AddAsync(newPartnerTree);
            
            // Invalidate cache et recharger pour les règles de groupe
            _memoryCache.Remove(CACHE_KEY);
            await LoadPartnerTreesAsync();
            
            // Maintenant appliquer les règles de groupe avec la liste complète
            if (CanModifyPartnerGroupCodeAsync(newPartnerTree, (await LoadPartnerTreesAsync()).ToList()))
            {
                newPartnerTree.PartnerGroupCode = partnerTree.PartnerGroupCode;
                await _partnerTreeRepository.UpdateAsync(newPartnerTree);
            }
            
            return await GetPartnerTreeByCodeAsync(partnerTree.Code);
        }

        public async Task<bool> DeletePartnerTreeAsync(string code)
        {
            // For delete operations, we need to find the partner tree even if it's already deleted
            var partnerTree = await GetPartnerTreeByCodeIncludingDeletedAsync(code);
            if (partnerTree == null) return false;
            
            // TODO : Check if there is children before delete

            await _partnerTreeRepository.Delete(partnerTree);
            
            // Invalidate cache
            _memoryCache.Remove(CACHE_KEY);
            
            return true;
        }

        private async Task<UNOPSPartnerTree?> GetPartnerTreeByCodeIncludingDeletedAsync(string code)
        {
            var allPartnerTreesIncludingDeleted = await _partnerTreeRepository.GetAllSortedAsync("Type");
            return allPartnerTreesIncludingDeleted.FirstOrDefault(pt => pt.Code == code);
        }

        private bool CanModifyPartnerCategoryCodeAsync(UNOPSPartnerTree partnerTree)
        {
            // Condition 1: Level_1 and not in specialCategoryCodes
            if (partnerTree.Type == LEVEL_1 && !PartnerTree.specialCategoryCodes.Contains(partnerTree.Code))
            {
                return true;
            }

            // Condition 2: Is a child of any specialCategoryCodes
            if (partnerTree.Type == LEVEL_2 && PartnerTree.specialCategoryCodes.Contains(partnerTree.Parent))
            {
                return true;
            }

            return false;
        }

        private bool CanModifyPartnerGroupCodeAsync(UNOPSPartnerTree partnerTree, List<UNOPSPartnerTree> partnerTrees)
        {
            // Condition 1: has a parent
            if (string.IsNullOrEmpty(partnerTree.Parent))
            {
                return false;
            }
            
            var parentPartnerTree = partnerTrees.FirstOrDefault(pt => pt.Code == partnerTree.Parent);
            if (parentPartnerTree == null)
            {
                return false;
            }

            // Condition 2: is a child of a category
            if (CanModifyPartnerCategoryCodeAsync(parentPartnerTree))
            {
                return true;
            }

            if (CanModifyPartnerGroupCodeAsync(parentPartnerTree, partnerTrees))
            {
                return true;
            }

            return false;
        }

        public async Task<List<int>> GetAllDescendantsAsync(string parentCode)
        {
            var allPartnerTrees = await LoadPartnerTreesAsync();
            var descendants = new List<int>();
            await GetDescendantsRecursive(parentCode, allPartnerTrees.ToList(), descendants);
            return descendants;
        }

        private async Task GetDescendantsRecursive(string parentCode, List<UNOPSPartnerTree> allPartnerTrees, List<int> descendants)
        {
            var children = allPartnerTrees.Where(pt => pt.Parent == parentCode && !pt.IsDeleted).ToList();
            
            foreach (var child in children)
            {
                descendants.Add(child.Id);
                await GetDescendantsRecursive(child.Code, allPartnerTrees, descendants);
            }
        }

        
    }
}
