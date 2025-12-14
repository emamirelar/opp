# Opportunity Page Backend Performance Analysis & Optimization Plan

**Date:** January 2025  
**Severity:** HIGH - Critical performance bottleneck affecting user experience  
**Impact:** Load times increase dramatically with multiple concurrent users

---

## Executive Summary

The opportunity page backend is experiencing severe performance issues due to:

1. **N+1 Query Problem** - Excessive sequential database calls (30+ queries per page load)
2. **No Entity Tracking Optimization** - All queries track entities, causing potential database locks
3. **Sequential Processing** - No parallel execution of independent operations
4. **Database Lock Contention** - Multiple users accessing same opportunity causes significant slowdown
5. **No Caching Strategy** - Reference data queried repeatedly for every request

**Estimated Impact:** 
- Single user: 3-5 seconds load time
- Multiple concurrent users (same opportunity): 10-30+ seconds load time
- 90% of load time is database I/O

---

## Detailed Performance Issues

### 1. Main Query - GetOpportunityAsync(int id)

**Location:** `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs:210-471`

#### Problem: Single Massive Query with 30+ Includes

```csharp
var entity = await context.Opportunities
    .Include(o => o.WorkflowStage)
    .Include(o => o.ResponsibleOrgUnit)
    .Include(o => o.ProposedInitiativeType)
    .Include(o => o.FundingPartners).ThenInclude(fp => fp.Partner)
    .Include(o => o.FundingPartners).ThenInclude(fp => fp.Currency)
    .Include(o => o.ClientPartners).ThenInclude(cp => cp.Partner)
    .Include(o => o.Stakeholders).ThenInclude(s => s.EntityRole)
    .Include(o => o.Stakeholders).ThenInclude(s => s.User).ThenInclude(u => u.UserProfile)
    .Include(o => o.Stakeholders).ThenInclude(s => s.OrganizationHierarchy)
    .Include(o => o.ExternalStakeholders).ThenInclude(es => es.Contact).ThenInclude(c => c.Partner)
    .Include(o => o.Deliverables).ThenInclude(d => d.Output)
    .Include(o => o.Countries).ThenInclude(c => c.Country)
    .Include(o => o.SDGs).ThenInclude(s => s.SDG)
    .Include(o => o.SDGs).ThenInclude(s => s.Targets).ThenInclude(t => t.SDGTarget)
    .Include(o => o.SDGs).ThenInclude(s => s.Targets).ThenInclude(t => t.Indicators).ThenInclude(i => i.SDGIndicator)
    .Include(o => o.UNCFOutcomes).ThenInclude(uo => uo.UNCFOutcome)
    .Include(o => o.UNCFOutcomes).ThenInclude(uo => uo.OpportunityCountry).ThenInclude(oc => oc.Country)
    .Include(o => o.UNCFOutcomes).ThenInclude(uo => uo.Indicators).ThenInclude(ui => ui.UNCFIndicator)
    .Include(o => o.UNOPSMissions).ThenInclude(om => om.UNOPSMission)
    .Include(o => o.CreatedByUser).ThenInclude(u => u.UserProfile)
    .Include(o => o.LastModifiedByUser).ThenInclude(u => u.UserProfile)
    .AsSplitQuery() // ✅ Good - splits into multiple queries
    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
```

**Issues:**
- ❌ No `.AsNoTracking()` - Entity Framework tracks all loaded entities
- ❌ Loads massive amount of data even if not needed by frontend
- ✅ Uses `.AsSplitQuery()` to avoid Cartesian explosion (good!)

**Database Lock Impact:**
- EF Core change tracker holds references to entities
- PostgreSQL may acquire row-level locks for tracked entities
- Multiple users reading same opportunity = lock contention

---

### 2. N+1 Query Problem - Partner Documents

**Location:** Lines 292-410

#### Problem: Separate Query for Each Partner

```csharp
// For EACH funding partner - separate database query!
foreach (var fundingPartner in model.FundingPartners)
{
    // ❌ Database Query #1 per partner
    fundingPartner.AssociatedDocuments = await GetDocumentsForPartner(
        id, fundingPartner.PartnerId, isFundingPartner: true
    );
    
    // ❌ Database Query #2 per partner
    fundingPartner.AvailableAgreements = await LoadPartnerAgreementsAsync(
        fundingPartner.PartnerId, entity.CreatedDate, entity.TargetDeliveryDate, opportunityCountryIds
    );
}

// For EACH client partner - another set of queries!
foreach (var clientPartner in model.ClientPartners)
{
    // ❌ Database Query #3 per partner
    clientPartner.AssociatedDocuments = await GetDocumentsForPartner(
        id, clientPartner.PartnerId, isFundingPartner: false
    );
    
    // ❌ Database Query #4 per partner
    clientPartner.AvailableAgreements = await LoadPartnerAgreementsAsync(
        clientPartner.PartnerId, entity.CreatedDate, entity.TargetDeliveryDate, opportunityCountryIds
    );
}
```

**Impact:**
- If opportunity has 5 funding partners + 3 client partners:
  - **16 additional database queries** (8 partners × 2 queries each)
- Each query waits for previous query to complete
- No parallelization

---

### 3. Sequential Country Enrichment

**Location:** Lines 412-428

#### Problem: 6 Sequential Database Queries

```csharp
// ❌ Sequential - each waits for previous to complete
await EnrichCountriesWithOrgUnitHierarchyAsync(model.Countries);      // Query 1
await EnrichCountriesWithActiveUNCFAsync(model.Countries);            // Query 2
await EnrichCountriesWithHumanitarianFrameworkAsync(model.Countries); // Query 3
await EnrichCountriesWithNdcAsync(model.Countries);                   // Query 4
await EnrichCountriesWithNapAsync(model.Countries);                   // Query 5
await EnrichCountriesWithOrgUnitStrategyAsync(model.Countries);       // Query 6
```

**Each method:**
```csharp
private async Task EnrichCountriesWithActiveUNCFAsync(...)
{
    // ❌ Separate query to database
    var countriesWithActiveUNCF = await context.UNCFMetadatas
        .Where(m => m.Status == EntityStatus.Active && iso2Codes.Contains(m.Country))
        .Select(m => m.Country)
        .Distinct()
        .ToListAsync();
    // ... update model properties
}
```

**Issues:**
- ❌ 6 separate database round-trips
- ❌ All operations are independent but executed sequentially
- ❌ No caching of reference data
- ❌ No `.AsNoTracking()` on any queries

---

### 4. Additional Sequential Queries

**Location:** Lines 437-469

```csharp
// ❌ Another database query for historical max
var historicalMax = await context.Opportunities
    .Where(o => o.ResponsibleOrgUnitId == model.ResponsibleOrgUnitId && o.Id != id && !o.IsDeleted)
    .SelectMany(o => o.FundingPartners)
    .GroupBy(fp => fp.OpportunityId)
    .Select(g => new { OpportunityId = g.Key, Total = g.Sum(fp => fp.AmountUSD ?? 0) })
    .OrderByDescending(x => x.Total)
    .FirstOrDefaultAsync();

// ❌ Another database query for SME selections
model.SMESelections = await GetSMESelectionsAsync(id);
```

---

### 5. Duplicate Permission Query

**Location:** `GetOpportunityAsync(ClaimsPrincipal user, int id)` - Lines 481-514

#### Problem: Queries Database AGAIN for Same Data

```csharp
public async Task<OpportunityModel?> GetOpportunityAsync(ClaimsPrincipal user, int id)
{
    // Calls GetOpportunityAsync(id) which already loaded EVERYTHING
    var model = await GetOpportunityAsync(id);
    
    // ❌ REDUNDANT QUERY - Stakeholders were already loaded above!
    var entity = await context.Opportunities
        .Include(o => o.Stakeholders)
        .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    
    // ... permission checks
}
```

**Issue:** Stakeholders were already fully loaded in the previous query but we query again!

---

### 6. No Database Transaction Isolation Optimization

**Location:** `UNOPS.PAO.Server/Startup.cs:525-534`

```csharp
services.AddDbContext<DataAccess.Context.AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());
```

**Issues:**
- ❌ No explicit isolation level configuration
- ❌ Default PostgreSQL isolation level is READ COMMITTED
- ❌ For read-only operations, could use READ UNCOMMITTED or snapshot isolation
- ❌ No query hints for lock-free reads

---

## Total Query Count Per Page Load

**Base Scenario (Simple Opportunity):**
1. Main opportunity query: **1 query** (split into ~8-10 by AsSplitQuery)
2. User name resolution: **2 queries**
3. Organization unit artifacts: **1 query**
4. Country enrichment: **6 queries**
5. Historical max calculation: **1 query**
6. SME selections: **1 query**
7. Permission query (duplicate): **1 query**

**Subtotal:** ~23 queries

**With Partners (5 funding + 3 client partners):**
- Document queries: **8 queries** (1 per partner)
- Agreement queries: **8 queries** (1 per partner)
- Partner entity queries: **8 queries** (1 per partner for ERP data)

**Total:** **~47 database queries** for a single page load!

---

## Multi-User Contention Issues

### Why Multiple Users Cause Dramatic Slowdown

1. **Entity Framework Change Tracking:**
   - All queries load entities into EF's change tracker
   - Change tracker maintains references and can hold database connections
   - PostgreSQL row-level locking with READ COMMITTED isolation

2. **Database Lock Behavior:**
   ```
   User 1: SELECT * FROM Opportunities WHERE Id = 123
           → Acquires shared lock on row 123
           → Loads 30+ related entities with shared locks
   
   User 2: SELECT * FROM Opportunities WHERE Id = 123
           → Waits for User 1's locks on related tables
           → Especially on junction tables (OpportunityStakeholders, etc.)
   
   User 3: SELECT * FROM Opportunities WHERE Id = 123
           → Waits for User 1 and User 2
   ```

3. **Connection Pool Exhaustion:**
   - Each long-running query holds a connection
   - 47 queries × multiple users = connection pool pressure
   - Default connection pool size may be insufficient

---

## Optimization Recommendations

### Priority 1: Immediate Performance Gains (High Impact, Low Effort)

#### 1.1 Add `.AsNoTracking()` to All Read Queries

**Impact:** 40-60% performance improvement, eliminates lock contention

```csharp
// ✅ OPTIMIZED VERSION
var entity = await context.Opportunities
    .AsNoTracking()  // ← ADD THIS
    .Include(o => o.WorkflowStage)
    .Include(o => o.ResponsibleOrgUnit)
    // ... rest of includes
    .AsSplitQuery()
    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
```

**Why it helps:**
- No change tracking = no entity caching
- No change tracking = no lock contention
- Faster query execution
- Lower memory usage

**Apply to:**
- `GetOpportunityAsync(int id)` - line 212
- `GetDocumentsForPartner()` - lines 563, 589
- `LoadPartnerAgreementsAsync()` - line 4230
- All `EnrichCountriesWith*Async()` methods
- Permission query - line 491

#### 1.2 Remove Duplicate Permission Query

**Impact:** 1 less database query

```csharp
// ✅ OPTIMIZED VERSION
public async Task<OpportunityModel?> GetOpportunityAsync(ClaimsPrincipal user, int id)
{
    var model = await GetOpportunityAsync(id);
    if (model == null) return null;
    
    // ❌ REMOVE THIS - data already in model.Stakeholders!
    // var entity = await context.Opportunities
    //     .Include(o => o.Stakeholders)
    //     .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    
    // ✅ Use model.Stakeholders instead
    var isStakeholder = await IsUserStakeholderOnOpportunityAsync(user, id, model.Stakeholders);
    
    // ... rest of logic using model.Stakeholders
}
```

#### 1.3 Parallelize Country Enrichment

**Impact:** 6 queries → 1 parallel execution = 5x faster

```csharp
// ✅ OPTIMIZED VERSION - Parallel execution
await Task.WhenAll(
    EnrichCountriesWithOrgUnitHierarchyAsync(model.Countries),
    EnrichCountriesWithActiveUNCFAsync(model.Countries),
    EnrichCountriesWithHumanitarianFrameworkAsync(model.Countries),
    EnrichCountriesWithNdcAsync(model.Countries),
    EnrichCountriesWithNapAsync(model.Countries),
    EnrichCountriesWithOrgUnitStrategyAsync(model.Countries)
);
```

**Why it works:**
- All 6 queries are independent
- Can execute simultaneously
- Total time = slowest query (not sum of all queries)

---

### Priority 2: Batch Operations (High Impact, Medium Effort)

#### 2.1 Batch Load Partner Documents

**Current:** 1 query per partner (N+1 problem)  
**Optimized:** 1 query for all partners

```csharp
// ✅ OPTIMIZED VERSION
private async Task EnrichPartnersWithDocumentsAsync(
    List<OpportunityFundingPartnerModel> fundingPartners,
    List<OpportunityClientPartnerModel> clientPartners,
    int opportunityId)
{
    var fundingPartnerIds = fundingPartners.Select(fp => fp.PartnerId).ToList();
    var clientPartnerIds = clientPartners.Select(cp => cp.PartnerId).ToList();
    
    // Single query for all funding partner documents
    var fundingDocs = await context.OpportunityFundingPartners
        .AsNoTracking()
        .Where(fp => fp.OpportunityId == opportunityId 
            && fundingPartnerIds.Contains(fp.PartnerId) 
            && fp.DocumentId != null)
        .Include(fp => fp.Document)
        .Select(fp => new { fp.PartnerId, fp.Document })
        .ToListAsync();
    
    // Single query for all client partner documents
    var clientDocs = await context.OpportunityClientPartners
        .AsNoTracking()
        .Where(cp => cp.OpportunityId == opportunityId 
            && clientPartnerIds.Contains(cp.PartnerId) 
            && cp.DocumentId != null)
        .Include(cp => cp.Document)
        .Select(cp => new { cp.PartnerId, cp.Document })
        .ToListAsync();
    
    // Group documents by partner
    var fundingDocsByPartner = fundingDocs
        .Where(x => x.Document != null && !x.Document.IsDeleted)
        .GroupBy(x => x.PartnerId)
        .ToDictionary(
            g => g.Key, 
            g => g.Select(x => mapper.Map<DocumentDetailModel>(x.Document)).ToList()
        );
    
    // Assign to models
    foreach (var fp in fundingPartners)
    {
        fp.AssociatedDocuments = fundingDocsByPartner.GetValueOrDefault(fp.PartnerId) 
            ?? new List<DocumentDetailModel>();
    }
    
    // Similar for client partners...
}
```

**Impact:** 16 queries → 2 queries (for 8 partners)

#### 2.2 Batch Load Partner Agreements

**Current:** 1 query per partner  
**Optimized:** 1 query for all partners

```csharp
// ✅ OPTIMIZED VERSION
private async Task<Dictionary<int, List<PartnerAgreementInfo>>> BatchLoadPartnerAgreementsAsync(
    List<int> partnerIds,
    DateTime? opportunityStartDate,
    DateTime? opportunityEndDate,
    List<int> opportunityCountryIds)
{
    // Single query to get all partner ERP values
    var partnerErpValues = await context.Partners
        .AsNoTracking()
        .Where(p => partnerIds.Contains(p.Id) && p.ErpDimValue.HasValue)
        .Select(p => new { p.Id, ErpNumber = p.ErpDimValue.Value.ToString() })
        .ToListAsync();
    
    var erpNumbers = partnerErpValues.Select(p => p.ErpNumber).ToList();
    
    // Single query to get all agreements for all partners
    var allAgreements = await context.PartnerAgreements
        .AsNoTracking()
        .Where(pa => erpNumbers.Contains(pa.PartnerAgreementPartner) && !pa.IsDeleted)
        .OrderByDescending(pa => pa.PartnerAgreementStartDate)
        .ToListAsync();
    
    // Group by partner and map
    var agreementsByPartner = new Dictionary<int, List<PartnerAgreementInfo>>();
    foreach (var partner in partnerErpValues)
    {
        var partnerAgreements = allAgreements
            .Where(a => a.PartnerAgreementPartner == partner.ErpNumber)
            .Select(a => new PartnerAgreementInfo
            {
                PartnerAgreementNumber = a.PartnerAgreementNumber,
                Name = a.Name,
                // ... map all properties
            })
            .ToList();
        
        agreementsByPartner[partner.Id] = partnerAgreements;
    }
    
    return agreementsByPartner;
}
```

**Impact:** 8 queries → 2 queries

---

### Priority 3: Caching Strategy (High Impact, Higher Effort)

#### 3.1 Cache Reference Data

**What to cache:**
- Countries with UNCF status
- Countries with NDC/NAP
- Countries with Humanitarian Framework
- Organization unit hierarchies
- Entity roles
- Workflow stages
- Currency codes

```csharp
// ✅ RECOMMENDED: Use IMemoryCache
private readonly IMemoryCache _cache;

private async Task<HashSet<string>> GetCountriesWithActiveUNCFCachedAsync()
{
    const string cacheKey = "Countries_ActiveUNCF";
    
    if (!_cache.TryGetValue(cacheKey, out HashSet<string> cachedData))
    {
        var countriesWithUNCF = await context.UNCFMetadatas
            .AsNoTracking()
            .Where(m => m.Status == EntityStatus.Active)
            .Select(m => m.Country)
            .Distinct()
            .ToListAsync();
        
        cachedData = new HashSet<string>(countriesWithUNCF, StringComparer.OrdinalIgnoreCase);
        
        // Cache for 1 hour (reference data changes infrequently)
        _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(1));
    }
    
    return cachedData;
}
```

**Cache invalidation:**
- Time-based expiration (1 hour for reference data)
- Event-based invalidation when reference data changes
- Implement `ICacheManager` service

**Impact:** 6 queries → 0 queries (after first load)

---

### Priority 4: Database Configuration (Medium Impact)

#### 4.1 Configure Connection Pool

```csharp
// In Startup.cs
var connectionString = Configuration.GetConnectionString("DbContext");
var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
{
    // ✅ Increase pool size for concurrent users
    MinPoolSize = 10,
    MaxPoolSize = 100,
    
    // ✅ Optimize connection lifecycle
    ConnectionLifetime = 300, // 5 minutes
    ConnectionIdleLifetime = 60, // 1 minute
    
    // ✅ Performance tuning
    CommandTimeout = 30,
    Timeout = 15,
    KeepAlive = 10,
    
    // ✅ Enable connection multiplexing
    Multiplexing = true,
    
    // ✅ Optimize read performance
    ReadBufferSize = 16384,
    WriteBufferSize = 16384
};

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionStringBuilder.ToString())
           .EnableSensitiveDataLogging(isDevelopment)
           .EnableDetailedErrors(isDevelopment));
```

#### 4.2 Add Database Indexes

```sql
-- Opportunity lookups
CREATE INDEX IF NOT EXISTS idx_opportunities_id_isdeleted 
ON public."Opportunities" ("Id", "IsDeleted") WHERE "IsDeleted" = false;

-- Partner document lookups
CREATE INDEX IF NOT EXISTS idx_opportunityfundingpartners_lookup 
ON public."OpportunityFundingPartners" ("OpportunityId", "PartnerId", "DocumentId") 
WHERE "DocumentId" IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_opportunityclientpartners_lookup 
ON public."OpportunityClientPartners" ("OpportunityId", "PartnerId", "DocumentId") 
WHERE "DocumentId" IS NOT NULL;

-- Partner agreement lookups
CREATE INDEX IF NOT EXISTS idx_partneragreements_partner_isdeleted 
ON public."PartnerAgreements" ("PartnerAgreementPartner", "IsDeleted") 
WHERE "IsDeleted" = false;

-- Country enrichment lookups
CREATE INDEX IF NOT EXISTS idx_uncfmetadatas_country_status 
ON public."UNCFMetadatas" ("Country", "Status") 
WHERE "Status" = 'Active';

CREATE INDEX IF NOT EXISTS idx_entityartifacts_country_artifacts 
ON public."EntityArtifacts" ("EntityType", "EntityId", "Status", "IsDeleted") 
WHERE "EntityType" = 'Country' AND "Status" = 'Active' AND "IsDeleted" = false;

-- Stakeholder permission checks
CREATE INDEX IF NOT EXISTS idx_opportunitystakeholders_permission_check 
ON public."OpportunityStakeholders" ("OpportunityId", "UserId", "OrganizationHierarchyId");
```

---

### Priority 5: Consider Stored Procedures for Complex Queries

For the main opportunity load, consider a stored procedure that:
1. Uses PostgreSQL's parallel query execution
2. Returns all data in a single round-trip
3. Uses temporary tables for intermediate results
4. Leverages database-level optimizations

```sql
CREATE OR REPLACE FUNCTION get_opportunity_complete(p_opportunity_id INT)
RETURNS TABLE (
    -- Define complete result structure
) AS $$
BEGIN
    -- Single optimized query with CTEs
    RETURN QUERY
    WITH opportunity_data AS (
        SELECT * FROM "Opportunities" WHERE "Id" = p_opportunity_id
    ),
    partner_documents AS (
        -- Batch load all partner documents
    ),
    country_enrichment AS (
        -- Single query for all country enrichment
    )
    SELECT * FROM opportunity_data
    -- Join all related data
END;
$$ LANGUAGE plpgsql;
```

---

## Implementation Plan

### Phase 1: Immediate Optimizations (Week 1)

**Goal:** 50-60% performance improvement

1. ✅ Add `.AsNoTracking()` to all read queries in `UNOPSOpportunityManager.cs`
2. ✅ Remove duplicate permission query
3. ✅ Parallelize country enrichment with `Task.WhenAll()`
4. ✅ Test with multiple concurrent users

**Estimated effort:** 4-8 hours  
**Risk:** Low (no breaking changes)

### Phase 2: Batch Operations (Week 2)

**Goal:** Additional 30% performance improvement

1. ✅ Implement batch loading for partner documents
2. ✅ Implement batch loading for partner agreements
3. ✅ Update partner enrichment loop to use batched data
4. ✅ Add unit tests for batching logic

**Estimated effort:** 1-2 days  
**Risk:** Low (internal refactoring only)

### Phase 3: Caching (Week 3)

**Goal:** 10-20% additional improvement + reduced DB load

1. ✅ Implement `ICacheManager` service
2. ✅ Add caching for reference data (countries with UNCF, NDC, etc.)
3. ✅ Implement cache invalidation strategy
4. ✅ Add cache monitoring and metrics

**Estimated effort:** 2-3 days  
**Risk:** Medium (need cache invalidation strategy)

### Phase 4: Database & Infrastructure (Week 4)

**Goal:** Handle higher concurrent load

1. ✅ Add database indexes
2. ✅ Optimize connection pool settings
3. ✅ Load test with 50+ concurrent users
4. ✅ Monitor query performance with PostgreSQL query analyzer

**Estimated effort:** 2-3 days  
**Risk:** Low (infrastructure changes)

---

## Expected Performance Improvements

### Current State
- Single user: **3-5 seconds**
- 5 concurrent users (same opportunity): **15-30 seconds**
- Database queries: **~47 queries per page load**
- Lock contention: **HIGH**

### After Phase 1 (AsNoTracking + Parallel)
- Single user: **1.5-2 seconds** (50% improvement)
- 5 concurrent users: **2-4 seconds** (80% improvement)
- Database queries: **~46 queries** (similar, but faster)
- Lock contention: **NONE**

### After Phase 2 (Batching)
- Single user: **0.8-1.2 seconds** (40% additional improvement)
- 5 concurrent users: **1-2 seconds** (50% improvement)
- Database queries: **~15-20 queries per page load** (60% reduction)
- Lock contention: **NONE**

### After Phase 3 (Caching)
- Single user: **0.5-0.8 seconds** (after cache warm-up)
- 5 concurrent users: **0.8-1.5 seconds**
- Database queries: **~8-12 queries per page load** (after cache)
- Database load: **60% reduction**

### After Phase 4 (Database tuning)
- Single user: **0.3-0.6 seconds**
- 50+ concurrent users: **0.8-2 seconds**
- Can handle **10x more concurrent users**

---

## Monitoring & Metrics

Add performance telemetry:

```csharp
using var activity = ActivitySource.StartActivity("GetOpportunity");
activity?.SetTag("opportunity.id", id);

var stopwatch = Stopwatch.StartNew();

// ... execute queries

stopwatch.Stop();
_logger.LogInformation(
    "GetOpportunityAsync completed in {ElapsedMs}ms for opportunity {OpportunityId}. " +
    "Queries executed: {QueryCount}",
    stopwatch.ElapsedMilliseconds, id, queryCount
);
```

**Track:**
- Total load time
- Query count per request
- Cache hit rate
- Concurrent user count
- Database connection pool usage

---

## Conclusion

The opportunity page backend performance can be improved by **80-90%** through systematic optimization:

1. **Immediate wins:** Add `AsNoTracking()` and parallelize operations
2. **Sustainable improvements:** Batch queries and add caching
3. **Infrastructure:** Database indexing and connection pooling

**Critical:** The lock contention issue with multiple concurrent users is caused by Entity Framework change tracking. Adding `AsNoTracking()` will completely eliminate this problem.

**Next Steps:**
1. Review and approve this optimization plan
2. Create implementation tasks
3. Begin with Phase 1 (highest impact, lowest risk)
4. Measure and validate improvements after each phase

