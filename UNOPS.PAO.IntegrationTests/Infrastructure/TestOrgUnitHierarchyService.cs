using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Collections.Generic;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

/// <summary>
/// Test-friendly implementation of IOrgUnitHierarchyService that works with in-memory database
/// </summary>
public class TestOrgUnitHierarchyService : IOrgUnitHierarchyService
{
    private readonly UNOPSAppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<int, List<int>> _testOverrides = new();

    public TestOrgUnitHierarchyService(UNOPSAppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// Sets test-specific descendants for an org unit, bypassing database hierarchy
    /// </summary>
    public void SetDescendants(int orgUnitId, List<int> descendantIds)
    {
        _testOverrides[orgUnitId] = descendantIds;
        // Clear cache for this org unit
        var cacheKey = $"org_hierarchy_{orgUnitId}";
        _cache.Remove(cacheKey);
    }

    public async Task<List<int>> GetDescendantIdsAsync(int orgUnitId)
    {
        // Check test overrides first
        if (_testOverrides.TryGetValue(orgUnitId, out var overrideIds))
            return overrideIds;
            
        var cacheKey = $"org_hierarchy_{orgUnitId}";
        
        if (_cache.TryGetValue(cacheKey, out List<int>? cachedIds) && cachedIds != null)
            return cachedIds;
        
        // In-memory database friendly implementation
        var ids = await GetDescendantsRecursive(orgUnitId);
        
        _cache.Set(cacheKey, ids, TimeSpan.FromHours(1));
        return ids;
    }
    
    private async Task<List<int>> GetDescendantsRecursive(int rootId)
    {
        var result = new List<int> { rootId };
        var toProcess = new Queue<int>();
        toProcess.Enqueue(rootId);
        
        while (toProcess.Count > 0)
        {
            var currentId = toProcess.Dequeue();
            var children = await _context.OrganizationHierarchies
                .Where(oh => oh.ParentId == currentId)
                .Select(oh => oh.Id)
                .ToListAsync();
            
            foreach (var childId in children)
            {
                result.Add(childId);
                toProcess.Enqueue(childId);
            }
        }
        
        return result;
    }
}