using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public class OrgUnitHierarchyService : IOrgUnitHierarchyService
{
    private readonly UNOPSAppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OrgUnitHierarchyService> _logger;

    public OrgUnitHierarchyService(UNOPSAppDbContext context, IMemoryCache cache, ILogger<OrgUnitHierarchyService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<int>> GetDescendantIdsAsync(int orgUnitId)
    {
        var cacheKey = $"org_hierarchy_{orgUnitId}";
        
        if (_cache.TryGetValue(cacheKey, out List<int>? cachedIds) && cachedIds != null)
        {
            return cachedIds;
        }
        
        // Requête récursive pour obtenir tous les descendants
        var ids = await GetDescendantsFromDb(orgUnitId);
        
        _cache.Set(cacheKey, ids, TimeSpan.FromHours(1));
        return ids;
    }
    
    private async Task<List<int>> GetDescendantsFromDb(int rootId)
    {
        var query = @"
            WITH RECURSIVE org_tree AS (
                SELECT ""Id"" FROM ""OrganizationHierarchies"" WHERE ""Id"" = @p0
                UNION ALL
                SELECT oh.""Id"" FROM ""OrganizationHierarchies"" oh
                INNER JOIN org_tree ot ON oh.""ParentId"" = ot.""Id""
            )
            SELECT ""Id"" FROM org_tree";
        
        var result = await _context.OrganizationHierarchies
            .FromSqlRaw(query, rootId)
            .Select(oh => oh.Id)
            .ToListAsync();
            
        return result;
    }
}