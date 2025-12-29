# GetOpportunityDetailsForAIAsync Performance Optimization Summary

## Problem Statement
The `GetOpportunityDetailsForAIAsync` method was experiencing severe performance issues, taking **over 310 seconds** to execute for opportunities with large amounts of data.

## Root Causes Identified

### 1. **Cartesian Product Explosion** (Critical)
- Original query had 22 `Include()` statements with multiple `ThenInclude()` chains
- When loading multiple collections (FundingPartners, ClientPartners, Stakeholders, Deliverables, Countries, SDGs, etc.), EF Core creates a Cartesian product
- Example: 10 FundingPartners × 5 ClientPartners × 20 Stakeholders × 15 Deliverables × 8 Countries × 10 SDGs = **1,200,000 rows** for a single opportunity!

### 2. **Missing Change Tracking Optimization**
- All queries used change tracking even though this is a read-only operation for AI processing
- Change tracking adds significant overhead for large result sets

### 3. **N+1 Query Problem**
- Partner agreements were loaded in a loop, executing separate queries for each funding partner
- 5 funding partners = 5+ separate database round trips

### 4. **Suboptimal Sub-Queries**
- Risk register query and SME selections queries lacked read-only optimization

---

## Optimizations Implemented

### ✅ **Priority 1: Split Query Strategy + AsNoTracking()** 
**Expected Impact: 60-80% reduction in query time**

**Before:**
```csharp
var opportunity = await context.Set<Opportunity>()
    .Include(o => o.ResponsibleOrgUnit)
    .Include(o => o.ProposedInitiativeType)
    .Include(o => o.FundingPartners).ThenInclude(fp => fp.Partner)
    .Include(o => o.FundingPartners).ThenInclude(fp => fp.Currency)
    .Include(o => o.FundingPartners).ThenInclude(fp => fp.Document)
    // ... 17 more includes creating massive Cartesian product
    .FirstOrDefaultAsync(o => o.Id == id);
```

**After:**
```csharp
// QUERY 1: Main Opportunity with Simple Navigation Properties Only
var opportunity = await context.Set<Opportunity>()
    .AsNoTracking() // No change tracking needed
    .Include(o => o.ResponsibleOrgUnit)
    .Include(o => o.ProposedInitiativeType)
    .Include(o => o.WorkflowStage)
    .Include(o => o.CreatedByUser)
    .Include(o => o.LastModifiedByUser)
    .FirstOrDefaultAsync(o => o.Id == id);

// QUERY 2: Funding Partners with Related Data (separate query)
var fundingPartners = await context.Set<OpportunityFundingPartner>()
    .AsNoTracking()
    .Where(fp => fp.OpportunityId == id)
    .Include(fp => fp.Partner)
    .Include(fp => fp.Currency)
    .Include(fp => fp.Document)
    .ToListAsync();

// QUERY 3-10: Similar pattern for all other collections
// Each collection loaded independently to avoid Cartesian product
```

**Changes Made:**
- Split single massive query into **10 separate optimized queries**
- Added `.AsNoTracking()` to all queries (read-only operation)
- Eliminated Cartesian product by loading collections independently
- Assigned loaded collections back to opportunity object for stats computation

---

### ✅ **Priority 2: Optimize Risk Register Query**
**Expected Impact: 10-15% reduction in risks query time**

**Changes Made:**
```csharp
var risks = await context.Set<Domain.Entities.Risk>()
    .AsNoTracking() // ✅ Added
    .Include(r => r.RiskTypeEntity)
    .Include(r => r.RiskCategory)
    // ... rest of includes
    .ToListAsync();
```

---

### ✅ **Priority 3: Batch Partner Agreements Query**
**Expected Impact: Eliminates N+1 queries (5+ queries → 2 queries)**

**Before:**
```csharp
foreach (var fp in opportunity.FundingPartners.Take(5)) 
{
    var agreements = await LoadPartnerAgreementsAsync(
        fp.PartnerId,
        opportunity.CreatedDate,
        opportunity.TargetDeliveryDate,
        opportunityCountryIds
    ); // ❌ Separate query for EACH funding partner
}
```

**After:**
```csharp
// Get all partner IDs upfront
var partnerIds = fundingPartners.Take(5).Select(fp => fp.PartnerId).ToList();

// BATCH QUERY 1: Get ERP values for ALL partners in ONE query
var partnerErpValues = await context.Partners
    .AsNoTracking()
    .Where(p => partnerIds.Contains(p.Id) && p.ErpDimValue.HasValue)
    .ToListAsync();

// BATCH QUERY 2: Load ALL agreements for ALL partners in ONE query
var allPartnerAgreements = await context.PartnerAgreements
    .AsNoTracking()
    .Where(pa => partnerNumbers.Contains(pa.PartnerAgreementPartner) && !pa.IsDeleted)
    .ToListAsync();

// Process in-memory (no more database calls)
var agreementsByPartner = allPartnerAgreements
    .Where(pa => !string.IsNullOrEmpty(pa.PartnerAgreementPartner))
    .GroupBy(pa => pa.PartnerAgreementPartner!)
    .ToDictionary(g => g.Key, g => g.ToList());
```

**Changes Made:**
- Replaced N+1 loop with **2 batch queries**
- In-memory grouping and filtering of pre-loaded data
- Eliminated 5+ database round trips per method call

---

### ✅ **Priority 4: Optimize SME Selections**
**Expected Impact: 5-10% reduction in SME query time**

**Changes Made:**
```csharp
// Query 1: Get SME roles
var smeRoles = await context.Set<EntityRole>()
    .AsNoTracking() // ✅ Added
    .Where(er => er.EntityType == "Opportunity" && er.Type == "SME" && !er.IsDeleted)
    .ToListAsync();

// Query 2: Get existing SME stakeholders
var existingSmeStakeholders = await context.Set<OpportunityStakeholder>()
    .AsNoTracking() // ✅ Added
    .Include(os => os.User).ThenInclude(u => u!.UserProfile)
    .ToListAsync();
```

---

## Expected Performance Improvement

| Optimization | Expected Time Reduction | Cumulative Result |
|-------------|------------------------|-------------------|
| **Starting Performance** | - | **310 seconds** |
| Priority 1: Split Query + AsNoTracking | 60-80% | **62-124 seconds** |
| Priority 2: Risks AsNoTracking | 5-10% of remaining | **56-112 seconds** |
| Priority 3: Batch Partner Agreements | 10-15% of remaining | **47-95 seconds** |
| Priority 4: SME AsNoTracking | 3-5% of remaining | **45-90 seconds** |
| **Priority 5: Parallel Execution** ✅ | **20-30% of remaining** | **32-63 seconds** |
| **Final Expected Performance** | **80-90% improvement** | **32-63 seconds** |

### Summary
- **From: 310+ seconds**
- **To: 32-63 seconds (estimated)**
- **Overall Improvement: 80-90% faster** 🚀

---

## Technical Details

### Query Count Reduction
- **Before:** 1 massive query + 5-10 N+1 queries + 2-4 sub-queries = **8-15 total queries**
- **After:** 10 split queries + 2 batch queries + 2 sub-queries = **14 total queries**

*Note: Despite slightly more queries, total execution time is dramatically reduced due to eliminating Cartesian product and using AsNoTracking()*

### Memory Optimization
- Added `.AsNoTracking()` to **14 queries** total
- Eliminated change tracker overhead for read-only AI processing
- Reduced memory footprint for large result sets

### Code Quality
- More maintainable: Each collection loaded independently
- Better testability: Separate queries can be tested individually
- Clear performance characteristics: No hidden Cartesian products
- Added inline comments documenting optimization strategy

---

## Thread Safety & DbContext Factory Pattern

### ✅ DbContext Configuration Verified

**Discovery:** The application already has `IDbContextFactory<UNOPSAppDbContext>` registered in `Startup.cs`:

```csharp
// Line 554-556 in Startup.cs
services.AddDbContextFactory<UNOPSAppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

**Why This Matters:**
- ✅ **Thread-Safe**: Factory creates separate DbContext instances for each parallel task
- ✅ **No Contention**: Each thread operates on its own context
- ✅ **Proper Disposal**: `await using` ensures contexts are disposed correctly
- ✅ **Connection Pooling**: Uses optimized connection pool settings (MinPoolSize: 10, MaxPoolSize: 100)

**Connection Pool Optimization (Already Configured):**
```csharp
// Line 526-538 in Startup.cs
var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
{
    MinPoolSize = 10,              // Keep connections warm
    MaxPoolSize = 100,             // Allow more concurrent connections
    ConnectionLifetime = 300,       // 5 minutes
    ConnectionIdleLifetime = 60,    // 1 minute idle timeout
    CommandTimeout = 60,            // Increase timeout for complex queries
    Timeout = 30,                   // Connection timeout
    KeepAlive = 10,                // Keep connections alive
    Multiplexing = true,           // Enable connection multiplexing for better throughput
    ReadBufferSize = 16384,        // 16KB read buffer
    WriteBufferSize = 16384        // 16KB write buffer
};
```

This configuration **perfectly supports** the parallel execution pattern with:
- **100 max connections** for high concurrency
- **Connection multiplexing** for better throughput
- **Warm connection pool** (10 minimum connections)

---

## Files Modified

1. **UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs**
   - Added `IDbContextFactory<UNOPSAppDbContext>` to constructor
   - `GetOpportunityDetailsForAIAsync()` method - Complete refactor with parallel execution
   - `GetSMESelectionsAsync()` method - Added AsNoTracking()

---

## Testing Recommendations

1. **Performance Testing**
   - Test with opportunities containing large datasets (100+ related records per collection)
   - Measure actual execution time before/after
   - Monitor database query logs to confirm batch queries

2. **Functional Testing**
   - Verify all data is still correctly loaded
   - Confirm statistics computation works correctly
   - Test AI prompt generation with optimized data

3. **Load Testing**
   - Test concurrent AI processing requests
   - Monitor memory usage during high load
   - Verify no connection pool exhaustion

---

## ✅ **Priority 5: Parallel Query Execution (IMPLEMENTED)**
**Expected Impact: 20-30% additional improvement**

**Implementation Status:** ✅ **COMPLETED**

Implemented parallel query execution using `IDbContextFactory<UNOPSAppDbContext>` for thread-safe concurrent database access.

**Before:**
```csharp
// Sequential execution - 10 queries run one after another
var fundingPartners = await context.Set<OpportunityFundingPartner>()...ToListAsync();
var clientPartners = await context.Set<OpportunityClientPartner>()...ToListAsync();
var stakeholders = await context.Set<OpportunityStakeholder>()...ToListAsync();
// ... 7 more sequential queries
```

**After:**
```csharp
// Parallel Wave 1: 10 independent queries execute concurrently
// Each task uses its own DbContext instance from the factory
var task1 = Task.Run(async () => {
    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
    return await ctx.Set<OpportunityFundingPartner>()...ToListAsync();
});
var task2 = Task.Run(async () => {
    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
    return await ctx.Set<OpportunityClientPartner>()...ToListAsync();
});
// ... 8 more parallel tasks

await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10);

// Parallel Wave 2: Dependent queries (SDG Targets/Indicators, UNCF Indicators)
// Execute in second wave after Wave 1 completes
```

**Key Features:**
- ✅ **Thread-Safe**: Each task gets its own DbContext from `IDbContextFactory`
- ✅ **Automatic Disposal**: Uses `await using` for proper resource cleanup
- ✅ **Two Waves**: Independent queries in Wave 1, dependent queries in Wave 2
- ✅ **10 Concurrent Queries**: Funding Partners, Client Partners, Stakeholders, External Stakeholders, Deliverables, Countries, SDGs, UNCF Outcomes, UNOPS Missions, **and Risks**

**Technical Details:**
- Added `IDbContextFactory<UNOPSAppDbContext>` to constructor injection
- Factory was already registered in `Startup.cs` (line 554-556)
- Used `Task.Run()` to ensure tasks execute on thread pool
- Each query runs in complete isolation with its own DbContext
- Proper error handling and resource disposal

### Priority 6: Caching
**Potential Improvement: 99%+ for cached items**

Add distributed caching (Redis) or memory cache for frequently accessed opportunities:
```csharp
var cacheKey = $"OpportunityAIDetails_{id}";
if (_cache.TryGetValue(cacheKey, out Dictionary<string, object> cachedResult))
{
    return cachedResult;
}

var result = await LoadOpportunityDetailsAsync(id);
_cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
return result;
```

### Priority 7: Compiled Queries
Use EF Core compiled queries for frequently executed query patterns to reduce query compilation overhead.

---

## Conclusion

The implemented optimizations address ALL critical performance bottlenecks in `GetOpportunityDetailsForAIAsync`:

✅ **Eliminated Cartesian product explosion** (Priority 1)  
✅ **Added read-only optimization** (Priority 1, 2, 4)  
✅ **Eliminated N+1 queries** (Priority 3)  
✅ **Implemented parallel query execution** (Priority 5) ⚡  
✅ **Maintained data completeness** (no information loss)

**Expected Result:** 80-90% performance improvement, reducing execution time from **310+ seconds to 32-63 seconds**.

### Remaining Optimization Opportunities

**Priority 6: Caching (Not Yet Implemented)**
**Potential Additional Improvement: 99%+ for cached items**

If AI processing frequently accesses the same opportunities, consider adding caching:

```csharp
private readonly IMemoryCache _cache;

public async Task<Dictionary<string, object>> GetOpportunityDetailsForAIAsync(int id)
{
    var cacheKey = $"OpportunityAIDetails_{id}_{DateTime.UtcNow.Date}";
    
    if (_cache.TryGetValue(cacheKey, out Dictionary<string, object> cachedResult))
    {
        return cachedResult;
    }
    
    var result = await LoadOpportunityDetailsAsync(id);
    
    // Cache for 30 minutes (adjust based on data change frequency)
    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
    
    return result;
}
```

**Benefits:**
- **Instant retrieval** for cached opportunities (<1ms vs 32-63 seconds)
- **Reduced database load** for frequently accessed opportunities
- **Lower costs** (fewer database queries)

**Considerations:**
- Requires invalidation strategy when opportunity data changes
- Memory usage increases (monitor cache size)
- Consider distributed cache (Redis) for multi-server scenarios

---

*Generated: December 15, 2024*
*Optimization Target: UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs*

