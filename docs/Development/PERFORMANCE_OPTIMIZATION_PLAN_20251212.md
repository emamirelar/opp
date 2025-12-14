# Opportunity Page Performance Optimization Plan

## Executive Summary

The opportunity page currently takes 20-40 seconds to load due to multiple performance bottlenecks:
- Duplicate API calls (5-7 duplicate requests)
- Large payloads (3MB+ total)
- Eager loading of all sections
- No caching mechanism
- Slow AI calls (25-40 seconds each)
- Duplicate AI calls

**Target**: Reduce initial load time to <5 seconds (75-87% improvement)

---

## Problem Analysis

### Current Performance Issues

1. **Duplicate API Calls**
   - `internal-users` (1.4MB) called twice
   - `proposed-initiative-types` called twice
   - `Opportunity` endpoint called multiple times
   - `insights` AI endpoint called twice (opportunity-view + analysis-section)

2. **Large Payloads**
   - `internal-users`: 1,481 kB
   - `contacts`: 336 kB
   - `outputs`: 313 kB
   - `partners`: 160 kB
   - Total: ~3MB+ on initial load

3. **Eager Loading**
   - All 9 section components load data in `ngOnInit`
   - Data loaded even when sections are not visible
   - No lazy loading strategy

4. **No Caching**
   - `ValuesService` makes fresh HTTP calls every time
   - No request deduplication
   - No frontend caching for AI responses

5. **Slow AI Calls**
   - `insights`: 36.54s (called twice = 73s total)
   - `dst-recommendations`: 40.47s
   - `similar-opportunities`: 25.27s
   - `similar-projects`: 33.57s
   - `relevant-people`: 34.27s
   - All AI calls block initial page render

---

## Solution Architecture

### Phase 1: Implement Caching Layer (High Impact)

**Priority**: Critical  
**Estimated Impact**: 50-70% reduction in network traffic  
**Risk**: Low

**File**: `UNOPS.PAO.ClientApp/src/app/shared/services/api/values.service.ts`

**Changes**:
- Add RxJS `shareReplay(1)` to all Observable methods to cache responses
- Implement TTL-based cache with configurable expiration (default: 5 minutes)
- Add cache invalidation methods for when data needs refresh
- Use `BehaviorSubject` pattern for frequently accessed data (partners, contacts, currencies)

**Implementation**:
```typescript
private cache = new Map<string, { data: any, timestamp: number, ttl: number }>();
private readonly DEFAULT_TTL = 5 * 60 * 1000; // 5 minutes

getPartners(): Observable<SimpleValue[]> {
  const cacheKey = 'partners';
  const cached = this.getCached(cacheKey);
  if (cached) return of(cached);
  
  return this.http.get<SimpleValue[]>(`${this.baseUrl}/partners`).pipe(
    tap(data => this.setCached(cacheKey, data)),
    shareReplay({ bufferSize: 1, refCount: false })
  );
}
```

**Benefits**:
- Eliminates duplicate calls
- Reduces network traffic by 50-70%
- Faster subsequent page loads

---

### Phase 2: Request Deduplication (High Impact)

**Priority**: Critical  
**Estimated Impact**: Prevents duplicate simultaneous requests  
**Risk**: Low

**File**: `UNOPS.PAO.ClientApp/src/app/shared/services/api/values.service.ts`

**Changes**:
- Implement request deduplication using RxJS operators
- When multiple components request same data simultaneously, share single HTTP request
- Use `shareReplay` with `refCount: false` to keep subscriptions alive

**Implementation**:
```typescript
private pendingRequests = new Map<string, Observable<any>>();

getInternalUsers(): Observable<SimpleValue[]> {
  const cacheKey = 'internal-users';
  
  // Check cache first
  const cached = this.getCached(cacheKey);
  if (cached) return of(cached);
  
  // Check if request is already pending
  if (this.pendingRequests.has(cacheKey)) {
    return this.pendingRequests.get(cacheKey)!;
  }
  
  // Create new request
  const request = this.http.get<SimpleValue[]>(`${this.baseUrl}/internal-users`).pipe(
    tap(data => {
      this.setCached(cacheKey, data);
      this.pendingRequests.delete(cacheKey);
    }),
    shareReplay({ bufferSize: 1, refCount: false })
  );
  
  this.pendingRequests.set(cacheKey, request);
  return request;
}
```

**Benefits**:
- Prevents duplicate simultaneous requests
- Reduces server load
- Faster response times

---

### Phase 3: Optimize AI Calls (High Impact)

**Priority**: Critical  
**Estimated Impact**: 50-70% reduction in AI-related load time  
**Risk**: Medium

**Files**:
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts`
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/analysis/opportunity-analysis-section.component.ts`
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.ts`
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/services/opportunity.service.ts`

**Issues Identified**:
- `insights` endpoint called **twice** (opportunity-view + analysis-section) - 36.54s each
- All AI calls take 25-40 seconds (backend LLM processing)
- No frontend caching for AI responses
- AI calls made even when sections not visible
- No progressive loading indicators for long-running AI calls

**Solutions**:

#### 3.1 Eliminate Duplicate AI Calls
- Remove `_loadSuggestions()` from `opportunity-view.component.ts` (duplicate of analysis-section)
- Share insights data between components using a shared service or signal
- Use single source of truth for AI insights

**Implementation**:
```typescript
// Remove from opportunity-view.component.ts
// private _loadSuggestions(): void { ... } // DELETE THIS

// Keep only in analysis-section.component.ts
// It already loads insights, no need to duplicate
```

#### 3.2 Frontend Caching for AI Responses
- Cache AI responses in `OpportunityService` with opportunity ID + timestamp as key
- Cache TTL: 10 minutes for insights, 30 minutes for DST recommendations (less volatile)
- Invalidate cache when opportunity data changes (section saves)

**Implementation**:
```typescript
// opportunity.service.ts
private aiCache = new Map<string, { data: any, timestamp: number, ttl: number }>();

getInsights(id: number, forceRefresh: boolean = false): Observable<OpportunityInsightsResponse> {
  const cacheKey = `insights-${id}`;
  
  if (!forceRefresh) {
    const cached = this.getCached(cacheKey, 10 * 60 * 1000); // 10 min TTL
    if (cached) return of(cached);
  }
  
  return this.http.get<OpportunityInsightsResponse>(`${this.apiUrl}/${id}/insights`).pipe(
    tap(data => this.setCached(cacheKey, data, 10 * 60 * 1000)),
    shareReplay({ bufferSize: 1, refCount: false })
  );
}
```

#### 3.3 Lazy Load AI Calls
- Only load AI insights when Analysis section becomes visible (Intersection Observer)
- Only load DST AI data when DST section becomes visible
- Defer all AI calls until after initial page render

**Implementation**:
```typescript
// analysis-section.component.ts
constructor() {
  effect(() => {
    const opp = this.opportunity();
    if (opp && opp.id && !this.hasLoadedInsights()) {
      // Only load when section is visible
      this.checkVisibilityAndLoad();
    }
  });
}

private checkVisibilityAndLoad(): void {
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        this.loadInsights();
        observer.disconnect();
      }
    });
  }, { rootMargin: '200px' });
  
  const element = document.querySelector('[data-section-id="analysis"]');
  if (element) observer.observe(element);
}
```

#### 3.4 Progressive Loading & User Feedback
- Show skeleton loaders for AI sections while loading
- Display "Generating insights..." messages with estimated time
- Allow users to cancel/refresh long-running AI calls
- Show cached data immediately while refreshing in background

**Benefits**:
- Reduces AI-related load time by 50-70%
- Eliminates duplicate calls (100% reduction)
- Improves UX with progressive loading
- Faster perceived performance with cached data

---

### Phase 4: Lazy Load Section Data (High Impact)

**Priority**: High  
**Estimated Impact**: 60-80% reduction in initial load  
**Risk**: Medium

**Files**:
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts`
- All section components in `sections/` directory

**Changes**:
- Implement Intersection Observer-based lazy loading for sections
- Only load section data when section becomes visible (within viewport)
- Add `data-loaded` signal to track which sections have loaded
- Load critical sections (Overview, What) immediately, defer others

**Implementation**:
```typescript
// opportunity-view.component.ts
private sectionLoadStates = signal<Map<string, boolean>>(new Map());

private setupLazyLoading(): void {
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const sectionId = entry.target.getAttribute('data-section-id');
        if (sectionId && !this.sectionLoadStates().get(sectionId)) {
          this.loadSectionData(sectionId);
          this.sectionLoadStates.update(map => new Map(map.set(sectionId, true)));
        }
      }
    });
  }, { rootMargin: '200px' });
  
  // Observe all section elements
  this.sections.forEach(section => {
    const element = document.querySelector(`[data-section-id="${section.id}"]`);
    if (element) observer.observe(element);
  });
}
```

**Benefits**:
- Reduces initial load by 60-80%
- Improves Time to Interactive
- Better user experience with progressive loading

---

### Phase 5: Backend Query Optimization (High Impact)

**Priority**: Critical  
**Estimated Impact**: 50-70% reduction in database query time  
**Risk**: Medium (requires careful testing)

**Backend Files**:
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs`
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs`
- `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs`

**Issues Identified**:
1. **Excessive Includes**: 20+ Include statements causing multiple round trips
2. **No Projection**: Loading full entities instead of only needed fields
3. **N+1 Queries**: Additional queries in loops (documents, user names, artifacts)
4. **No AsNoTracking()**: Entities tracked unnecessarily for read operations
5. **Sequential Queries**: Multiple separate queries executed sequentially
6. **Large Result Sets**: Loading all related data even when not needed

**Solutions**:

#### 5.1 Use Projection Instead of Full Entity Loading
- Project only required fields directly to DTOs
- Avoid loading full entity graphs when only specific fields needed
- Use `Select()` to project to DTOs directly

**Implementation**:
```csharp
// Instead of loading full entities and mapping
public async Task<OpportunityModel?> GetOpportunityAsync(int id)
{
    // Current: Loads full entities
    var entity = await context.Opportunities
        .Include(o => o.WorkflowStage)
        .Include(o => o.ResponsibleOrgUnit)
        // ... 20+ more includes
        .FirstOrDefaultAsync(o => o.Id == id);
    
    var model = mapper.Map<OpportunityModel>(entity);
    
    // Optimized: Project directly to DTO
    var model = await context.Opportunities
        .Where(o => o.Id == id && !o.IsDeleted)
        .Select(o => new OpportunityModel
        {
            Id = o.Id,
            Name = o.Name,
            Description = o.Description,
            WorkflowStageName = o.WorkflowStage != null ? o.WorkflowStage.Name : null,
            ResponsibleOrgUnitName = o.ResponsibleOrgUnit != null ? o.ResponsibleOrgUnit.Name : null,
            // ... only needed fields
            FundingPartners = o.FundingPartners.Select(fp => new FundingPartnerModel
            {
                Id = fp.Id,
                PartnerId = fp.PartnerId,
                PartnerName = fp.Partner != null ? fp.Partner.Name : null,
                // ... only needed fields
            }).ToList(),
            // ... other collections
        })
        .FirstOrDefaultAsync();
}
```

#### 5.2 Use AsNoTracking() for Read Operations
- Add `AsNoTracking()` to queries that don't need change tracking
- Reduces memory usage and improves performance

**Implementation**:
```csharp
var entity = await context.Opportunities
    .AsNoTracking() // Add this for read-only operations
    .Include(o => o.WorkflowStage)
    // ... rest of query
    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
```

#### 5.3 Batch Additional Queries
- Combine multiple separate queries into single batch queries
- Use `ToListAsync()` with `Where()` instead of loops

**Implementation**:
```csharp
// Current: N+1 queries in loop
foreach (var fundingPartner in model.FundingPartners)
{
    fundingPartner.AssociatedDocuments = await GetDocumentsForPartner(
        id, fundingPartner.PartnerId, isFundingPartner: true);
}

// Optimized: Single batch query
var partnerIds = model.FundingPartners.Select(fp => fp.PartnerId).ToList();
var allDocuments = await context.Documents
    .Where(d => d.EntityType == "Opportunity" 
        && d.EntityId == id
        && partnerIds.Contains(d.RelatedEntityId ?? 0))
    .ToListAsync();
    
// Group by partner in memory
var documentsByPartner = allDocuments
    .GroupBy(d => d.RelatedEntityId)
    .ToDictionary(g => g.Key ?? 0, g => g.ToList());
    
foreach (var fundingPartner in model.FundingPartners)
{
    documentsByPartner.TryGetValue(fundingPartner.PartnerId, out var docs);
    fundingPartner.AssociatedDocuments = docs ?? new List<DocumentModel>();
}
```

#### 5.4 Optimize User Name Resolution
- Batch user name lookups instead of individual queries
- Cache user names in memory during request

**Implementation**:
```csharp
// Current: Individual queries
model.CreatedByName = await GetUserNameByIdAsync(entity.CreatedBy);
model.LastModifiedByName = await GetUserNameByIdAsync(entity.LastModifiedBy);

// Optimized: Batch query
var userIds = new[] { entity.CreatedBy, entity.LastModifiedBy }
    .Where(id => id > 0)
    .Distinct()
    .ToList();
    
var users = await context.Users
    .Where(u => userIds.Contains(u.Id))
    .Select(u => new { u.Id, Name = u.UserProfile != null ? u.UserProfile.DisplayName : u.Email })
    .ToDictionaryAsync(u => u.Id, u => u.Name);
    
model.CreatedByName = users.GetValueOrDefault(entity.CreatedBy, "System");
model.LastModifiedByName = users.GetValueOrDefault(entity.LastModifiedBy, "System");
```

#### 5.5 Optimize EntityArtifacts Query
- Add indexes on EntityArtifacts table
- Use projection instead of full entity loading

**Implementation**:
```csharp
// Current: Loads full entities
var orgUnitArtifacts = await context.EntityArtifacts
    .Where(a => a.EntityType == "OrganizationHierarchy"
        && a.EntityId == entity.ResponsibleOrgUnit.Id
        && !a.IsDeleted
        && a.Status == EntityStatus.Active)
    .Include(a => a.ArtifactType)
        .ThenInclude(at => at!.ArtifactDataType)
    .OrderBy(a => a.ArtifactType!.Order)
    .ToListAsync();

// Optimized: Project directly
var orgUnitArtifacts = await context.EntityArtifacts
    .Where(a => a.EntityType == "OrganizationHierarchy"
        && a.EntityId == entity.ResponsibleOrgUnit.Id
        && !a.IsDeleted
        && a.Status == EntityStatus.Active)
    .Select(a => new
    {
        a.Id,
        a.Value,
        ArtifactTypeName = a.ArtifactType != null ? a.ArtifactType.Name : null,
        ArtifactTypeOrder = a.ArtifactType != null ? a.ArtifactType.Order : 0,
        DataTypeName = a.ArtifactType != null && a.ArtifactType.ArtifactDataType != null 
            ? a.ArtifactType.ArtifactDataType.Name : null
    })
    .OrderBy(a => a.ArtifactTypeOrder)
    .ToListAsync();
```

#### 5.6 Add Database Indexes
- Add indexes on frequently queried columns
- Index foreign keys used in joins

**Migration Example**:
```csharp
// Add indexes for common queries
modelBuilder.Entity<Opportunity>()
    .HasIndex(o => new { o.Id, o.IsDeleted })
    .HasDatabaseName("IX_Opportunity_Id_IsDeleted");

modelBuilder.Entity<EntityArtifact>()
    .HasIndex(a => new { a.EntityType, a.EntityId, a.IsDeleted, a.Status })
    .HasDatabaseName("IX_EntityArtifact_Type_Id_Deleted_Status");
```

#### 5.7 Implement Query Result Caching
- Cache opportunity data at manager level
- Use memory cache with TTL for frequently accessed opportunities
- Invalidate cache on updates

**Implementation**:
```csharp
private readonly IMemoryCache _cache;
private readonly MemoryCacheEntryOptions _cacheOptions = new()
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    SlidingExpiration = TimeSpan.FromMinutes(2)
};

public async Task<OpportunityModel?> GetOpportunityAsync(int id)
{
    var cacheKey = $"opportunity_{id}";
    
    if (_cache.TryGetValue(cacheKey, out OpportunityModel? cached))
    {
        return cached;
    }
    
    var model = await LoadOpportunityFromDatabase(id);
    
    if (model != null)
    {
        _cache.Set(cacheKey, model, _cacheOptions);
    }
    
    return model;
}
```

**Benefits**:
- 50-70% reduction in database query time
- Reduced database load
- Faster response times
- Better scalability

---

### Phase 6: Optimize Large Payloads (Medium Impact)

**Priority**: Medium  
**Estimated Impact**: 70-90% reduction in payload sizes  
**Risk**: Medium (requires backend changes)

**Backend Files**:
- `UNOPS.PAO.Presentation/Controllers/Shared/ValuesController.cs`
- `UNOPS.PAO.Business/Managers/ValuesManager.cs`

**Frontend Files**:
- `UNOPS.PAO.ClientApp/src/app/shared/services/api/values.service.ts`

**Changes**:
- Implement pagination for `internal-users` endpoint (currently 1.4MB)
- Add filtering/search parameters to reduce payload size
- Return only essential fields for dropdowns (id, name) instead of full objects
- Implement server-side filtering for contacts/partners based on opportunity context

**Backend Implementation**:
```csharp
// ValuesController.cs
[HttpGet(APIDictionary.InternalUsers)]
public async Task<ActionResult> GetInternalUsers(
    [FromQuery] int pageSize = 100,
    [FromQuery] string? searchTerm = null,
    [FromQuery] bool activeOnly = true)
{
    // Return paginated, filtered results
    // Only include id, name, email for dropdowns
    var query = _manager.GetUsers()
        .Where(u => !activeOnly || u.IsActive);
        
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm));
    }
    
    var users = await query
        .Select(u => new { u.Id, u.Name, u.Email })
        .Take(pageSize)
        .ToListAsync();
        
    return Ok(users);
}
```

**Benefits**:
- Reduces payload sizes by 70-90%
- Faster response times
- Better scalability

---

### Phase 7: Batch Critical Requests (Medium Impact)

**Priority**: Medium  
**Estimated Impact**: Better perceived performance  
**Risk**: Low

**File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts`

**Changes**:
- Create a data preloading service that batches critical requests
- Load opportunity data + essential dropdowns in parallel using `forkJoin`
- Prioritize above-the-fold content (Overview section data)
- Defer non-critical data (DST analysis, AI insights) until after initial render

**Implementation**:
```typescript
private loadCriticalData(): void {
  forkJoin({
    opportunity: this.opportunityService.getOpportunityById(+this.recordId),
    currencies: this.valuesService.getCurrencies(),
    countries: this.valuesService.getCountries()
  }).subscribe({
    next: (data) => {
      this.opportunity.set(data.opportunity);
      // Set other critical data
      this.loading.set(false);
    }
  });
}
```

**Benefits**:
- Better perceived performance
- Faster initial render
- Parallel loading of critical data

---

### Phase 8: Optimize DST Section Loading (Low-Medium Impact)

**Priority**: Low  
**Estimated Impact**: Reduces initial page load time  
**Risk**: Low

**File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.ts`

**Changes**:
- Current staggered loading (500ms, 1000ms, 1500ms, 2000ms) is good but can be improved
- Only load DST data when section is visible (lazy loading)
- Consider loading risks immediately but deferring AI-heavy calls (recommendations, similar items)

**Implementation**:
```typescript
// Only load when section visible
effect(() => {
  const opp = this.opportunity();
  if (opp && opp.id && this.isSectionVisible()) {
    this.loadDSTRisks(); // Immediate
    // Defer AI calls until user scrolls to section
    setTimeout(() => this.loadDSTRecommendations(), 500);
  }
});
```

**Benefits**:
- Reduces initial page load time
- Better user experience

---

## Expected Performance Improvements

| Metric | Current | Target | Improvement |
|--------|---------|--------|-------------|
| Initial Load Time | 20-40s | <5s | 75-87% |
| Network Requests | 20+ | 8-10 | 50-60% |
| Total Payload | ~3MB | <1MB | 67% |
| Time to Interactive | 30-45s | <8s | 73-82% |
| Duplicate Requests | 5-7 | 0 | 100% |
| AI Call Time | 25-40s | 5-15s (cached) / 20-30s (fresh) | 40-60% |
| Duplicate AI Calls | 1 (insights) | 0 | 100% |
| Database Query Time | 2-5s | 0.5-1.5s | 50-70% |
| N+1 Queries | Multiple | 0 | 100% |

---

## Implementation Order

### Phase 1-2: Foundation (Week 1)
1. Implement caching layer in `ValuesService`
2. Add request deduplication
3. **Impact**: Immediate 50-70% reduction in duplicate calls

### Phase 3: AI Optimization (Week 1-2)
1. Remove duplicate `insights` call from opportunity-view
2. Add frontend caching for AI responses
3. Implement lazy loading for AI calls
4. Add progressive loading indicators
5. **Impact**: 50-70% reduction in AI-related load time

### Phase 4: Lazy Loading (Week 2)
1. Implement Intersection Observer for sections
2. Convert section components to lazy-loaded
3. **Impact**: 60-80% reduction in initial load

### Phase 5: Backend Query Optimization (Week 2-3)
1. Implement projection instead of full entity loading
2. Add `AsNoTracking()` for read operations
3. Batch additional queries (documents, user names)
4. Optimize EntityArtifacts query
5. Add database indexes
6. Implement query result caching
7. **Impact**: 50-70% reduction in database query time

### Phase 6: Payload Optimization (Week 3)
1. Coordinate with backend team
2. Implement pagination for large endpoints
3. Add filtering/search parameters
4. **Impact**: 70-90% reduction in payload sizes

### Phase 7: Batching (Week 3)
1. Create data preloading service
2. Batch critical requests
3. **Impact**: Better perceived performance

### Phase 8: DST Optimization (Week 3)
1. Make DST section lazy-loaded
2. Defer AI-heavy calls
3. **Impact**: Final optimizations

---

## Testing Strategy

1. **Performance Testing**
   - Use Chrome DevTools Network tab to measure before/after
   - Monitor Lighthouse scores
   - Track Core Web Vitals (LCP, FID, CLS)

2. **Cache Validation**
   - Verify cached data is reused across component instances
   - Test cache invalidation on data changes
   - Verify cache expiration works correctly

3. **Lazy Loading**
   - Verify sections only load when scrolled into view
   - Test scroll behavior and section visibility detection
   - Ensure no data is missed on fast scrolling

4. **Error Handling**
   - Ensure cache failures don't break functionality
   - Test network failures and retry logic
   - Verify graceful degradation

5. **Memory Management**
   - Monitor for memory leaks with long-lived subscriptions
   - Test cache size limits and LRU eviction
   - Verify cleanup on component destruction

6. **AI Call Testing**
   - Verify duplicate calls are eliminated
   - Test cache hit/miss scenarios
   - Verify lazy loading works for AI sections
   - Test progressive loading indicators

---

## Files to Modify

### Frontend

**Core Services**:
- `UNOPS.PAO.ClientApp/src/app/shared/services/api/values.service.ts` - Add caching and deduplication
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/services/opportunity.service.ts` - Add AI response caching

**Components**:
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts` - Lazy loading coordinator, remove duplicate AI call
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/analysis/opportunity-analysis-section.component.ts` - Lazy load AI insights
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.ts` - Lazy load AI calls
- All section components in `sections/` directory - Convert to lazy-loaded data

### Backend

**Phase 5 (Query Optimization)**:
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` - Optimize GetOpportunityAsync query
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs` - Optimize GetOpportunityAsync query
- `UNOPS.PAO.UNOPSDataAccess/Migrations/` - Add database indexes

**Phase 6 (Payload Optimization)**:
- `UNOPS.PAO.Presentation/Controllers/Shared/ValuesController.cs` - Add pagination/filtering
- `UNOPS.PAO.Business/Managers/ValuesManager.cs` - Optimize queries

---

## Risk Mitigation

### Cache Invalidation
- Implement manual refresh mechanism for admin users
- Add cache invalidation on data mutations
- Use versioned cache keys for breaking changes

### Backward Compatibility
- Ensure cached data structure matches existing expectations
- Maintain API contract compatibility
- Gradual rollout with feature flags

### Error Recovery
- Fallback to direct HTTP calls if cache fails
- Implement retry logic for failed requests
- Graceful degradation for cache errors

### Memory Management
- Implement cache size limits (e.g., max 50 entries)
- Use LRU (Least Recently Used) eviction policy
- Monitor memory usage in production

### AI Call Reliability
- Verify backend caching is working correctly
- Monitor cache hit rates
- Implement request queuing to prevent backend overload
- Add timeout handling for long-running AI calls

---

## Success Metrics

### Performance Metrics
- **Initial Load Time**: <5 seconds (from 20-40s)
- **Time to Interactive**: <8 seconds (from 30-45s)
- **Network Requests**: 8-10 (from 20+)
- **Total Payload**: <1MB (from ~3MB)

### User Experience Metrics
- **First Contentful Paint (FCP)**: <1.5s
- **Largest Contentful Paint (LCP)**: <2.5s
- **Cumulative Layout Shift (CLS)**: <0.1
- **First Input Delay (FID)**: <100ms

### Business Metrics
- **Page Bounce Rate**: Monitor for improvements
- **User Engagement**: Track time on page
- **Error Rate**: Monitor for cache-related errors

---

## Notes

- All changes should be backward compatible
- Use feature flags for gradual rollout
- Monitor performance metrics in production
- Coordinate with backend team for Phase 5 changes
- Test thoroughly in staging before production deployment

---

## Database Optimization Checklist

### Indexes to Add
- [ ] `IX_Opportunity_Id_IsDeleted` on `Opportunity(Id, IsDeleted)`
- [ ] `IX_EntityArtifact_Type_Id_Deleted_Status` on `EntityArtifact(EntityType, EntityId, IsDeleted, Status)`
- [ ] `IX_Document_EntityType_EntityId` on `Document(EntityType, EntityId)`
- [ ] `IX_User_Id_IsActive` on `User(Id, IsActive)` (if not exists)
- [ ] `IX_OpportunityFundingPartner_OpportunityId_PartnerId` on `OpportunityFundingPartner(OpportunityId, PartnerId)`
- [ ] `IX_OpportunityClientPartner_OpportunityId_PartnerId` on `OpportunityClientPartner(OpportunityId, PartnerId)`

### Query Patterns to Optimize
- [ ] Replace full entity loading with projection in `GetOpportunityAsync`
- [ ] Add `AsNoTracking()` to all read-only queries
- [ ] Batch user name lookups instead of individual queries
- [ ] Batch document queries instead of loop-based queries
- [ ] Optimize EntityArtifacts query with projection
- [ ] Implement query result caching with 5-minute TTL

## Questions for Backend Team

1. **AI Caching**: What is the current cache TTL for AI responses? Can we extend it?
2. **Pagination**: Can we implement pagination for `internal-users` endpoint?
3. **Filtering**: Can we add search/filter parameters to reduce payload sizes?
4. **Streaming**: Can we implement streaming for long-running AI calls?
5. **Request Queuing**: Can we implement request queuing to prevent backend overload?
6. **Query Caching**: Can we implement Redis or in-memory caching for opportunity queries?
7. **Database Indexes**: Can we add the recommended indexes without impacting write performance?
8. **Projection Strategy**: Are there any concerns with using projection instead of full entity loading?

---

**Document Version**: 1.0  
**Last Updated**: 2025-01-XX  
**Status**: Planning Phase

