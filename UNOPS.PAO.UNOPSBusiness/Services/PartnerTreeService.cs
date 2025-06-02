using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
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
                partnerTrees = await _partnerTreeRepository.GetAllSortedAsync("Type");
                
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
            return allPartnerTrees.FirstOrDefault(pt => pt.Code == code);
        }

        public async Task<UNOPSPartnerTree?> GetPartnerCategoryByPartnerGroupCodeAsync(string code)
        {
            var partnerTrees = await LoadPartnerTreesAsync();
            var partnerTree = await GetPartnerTreeByCodeAsync(code);
            return await GetParentCategory(partnerTree, partnerTrees.ToList());
        }

        private async Task<UNOPSPartnerTree?> GetParentCategory(UNOPSPartnerTree partnerTree, List<UNOPSPartnerTree> partnerTrees)
        {
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
            return allPartnerTrees.FirstOrDefault(pt => pt.Id == id);
        }

        public async Task<bool> UpdatePartnerTreeAsync(UNOPSPartnerTree partnerTree)
        {
            if (partnerTree == null) throw new ArgumentNullException(nameof(partnerTree));
            
            // Find the existing partner tree
            var existingPartnerTree = await _partnerTreeRepository.GetByIdAsync(partnerTree.Id);
            if (existingPartnerTree == null) return false;

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
            existingPartnerTree.Parent = partnerTree.Parent;
            
            // Save changes
            await _partnerTreeRepository.UpdateAsync(existingPartnerTree);
            
            // Invalidate cache
            _memoryCache.Remove(CACHE_KEY);
            
            return true;
        }

        public async Task<UNOPSPartnerTree?> CreatePartnerTreeAsync(UNOPSPartnerTree partnerTree)
        {
            if (partnerTree == null) throw new ArgumentNullException(nameof(partnerTree));

            var newPartnerTree = new UNOPSPartnerTree
            {
                Code = partnerTree.Code,
                Description = partnerTree.Description,
                Type = partnerTree.Type,
                Parent = partnerTree.Parent
            };
            
            // Saving the partnerTree before check if it's allowed to save Partner Category or Partner Group
            // TODO: Should be improved
            await _partnerTreeRepository.AddAsync(newPartnerTree);
            
            // Invalidate cache
            _memoryCache.Remove(CACHE_KEY);

            await LoadPartnerTreesAsync();
            
            await UpdatePartnerTreeAsync(partnerTree);
            
            return await GetPartnerTreeByCodeAsync(partnerTree.Code);
        }

        public async Task<bool> DeletePartnerTreeAsync(string code)
        {
            var partnerTree = await GetPartnerTreeByCodeAsync(code);
            if (partnerTree == null) return false;
            
            // TODO : Check if there is children before delete

            await _partnerTreeRepository.Delete(partnerTree);
            
            // Invalidate cache
            _memoryCache.Remove(CACHE_KEY);
            
            return true;
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

        
    }
}
