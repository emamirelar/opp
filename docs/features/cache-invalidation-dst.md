# Cache Invalidation for DST Similar Projects and Relevant People

## Feature Overview

Added `invalidateCache` parameter support to both Similar Projects and Relevant People endpoints in the DST (Digital Strategy & Transformation) section. This allows users to force refresh data and bypass cached AI results.

## Motivation

During testing and development, cached Gemini AI responses prevented verification of:
- Vector store integration fixes
- Data validation improvements
- ID preservation logic
- Error handling updates

Users also need ability to force fresh results when:
- Opportunity details have changed significantly
- Vector store data has been updated
- Testing new AI prompt configurations
- Troubleshooting data issues

## Implementation

### Backend Changes

#### 1. Controller Endpoints (`OpportunityController.cs`)

Added `invalidateCache` query parameter to both endpoints:

```csharp
[HttpGet(APIDictionary.Opportunity + "/{id}/similar-projects")]
public async Task<ActionResult> GetSimilarProjects(
    int id, 
    [FromQuery] int maxResults = 6, 
    [FromQuery] bool invalidateCache = false)
{
    _logger.LogInformation(
        "Getting similar projects for opportunity {OpportunityId} with maxResults={MaxResults}, invalidateCache={InvalidateCache}", 
        id, maxResults, invalidateCache);
    
    var response = await _geminiManager.GetSimilarProjectsAsync(id, maxResults, user, invalidateCache);
    // ...
}

[HttpGet(APIDictionary.Opportunity + "/{id}/relevant-people")]
public async Task<ActionResult> GetRelevantPeople(
    int id, 
    [FromQuery] int maxResults = 6, 
    [FromQuery] bool invalidateCache = false)
{
    // Similar implementation...
}
```

#### 2. Manager Interface (`IGeminiManager.cs`)

Updated method signatures:

```csharp
Task<SimilarProjectsResponse> GetSimilarProjectsAsync(
    int opportunityId, 
    int maxResults = 10, 
    ClaimsPrincipal user = null, 
    bool invalidateCache = false);

Task<RelevantPeopleResponse> GetRelevantPeopleAsync(
    int opportunityId, 
    int maxResults = 10, 
    ClaimsPrincipal user = null, 
    bool invalidateCache = false);
```

#### 3. Manager Implementation (`UNOPSGeminiManager.cs`)

Pass `bypassCache` to AI service:

```csharp
public async Task<SimilarProjectsResponse> GetSimilarProjectsAsync(
    int opportunityId, 
    int maxResults = 6, 
    ClaimsPrincipal user = null, 
    bool invalidateCache = false)
{
    _logger.LogInformation(
        $"🔍 [SIMILAR-PROJECTS] Starting similar projects search for opportunity {opportunityId}, invalidateCache={invalidateCache}");
    
    // ... vector store search ...
    
    // Call Gemini with cache bypass
    var refineResponse = await _aiService.FetchResultFromGemini(
        refinePrompt, 
        refinedPrompt, 
        opportunityId.ToString(), 
        bypassCache: invalidateCache  // ✅ Pass through
    );
    
    // ...
}
```

### Frontend Changes

#### 1. Service Layer (`opportunity.service.ts`)

Added `invalidateCache` parameter to service methods:

```typescript
getSimilarProjects(
  id: number,
  maxResults: number = 10,
  invalidateCache: boolean = false,
): Observable<SimilarProjectsResponse> {
  return this.http.get<SimilarProjectsResponse>(
    `${this.apiUrl}/${id}/similar-projects`,
    {
      params: {
        maxResults: maxResults.toString(),
        invalidateCache: invalidateCache.toString(),
      },
    },
  );
}

getRelevantPeople(
  id: number,
  maxResults: number = 10,
  invalidateCache: boolean = false,
): Observable<RelevantPeopleResponse> {
  return this.http.get<RelevantPeopleResponse>(
    `${this.apiUrl}/${id}/relevant-people`,
    {
      params: {
        maxResults: maxResults.toString(),
        invalidateCache: invalidateCache.toString(),
      },
    },
  );
}
```

#### 2. Component Layer (`opportunity-dst-section.component.ts`)

Updated load methods to accept cache invalidation parameter:

```typescript
loadSimilarProjects(invalidateCache: boolean = false): void {
  const opportunityId = this.opportunity().id;
  this.loadingSimilarProjects.set(true);
  this.similarProjectsError.set(null);
  
  this.opportunityService.getSimilarProjects(opportunityId, 6, invalidateCache).subscribe({
    // ... handle response ...
  });
}

loadRelevantPeople(invalidateCache: boolean = false): void {
  // Similar implementation...
}
```

Updated refresh methods to force cache invalidation:

```typescript
refreshSimilarProjects(): void {
  // Clear existing data and reload with cache invalidation
  this.similarProjects.set(null);
  this.similarProjectsResponse.set(null);
  this.loadSimilarProjects(true); // ✅ Force cache bypass
}

refreshRelevantPeople(): void {
  // Clear existing data and reload with cache invalidation
  this.relevantPeople.set(null);
  this.relevantPeopleResponse.set(null);
  this.loadRelevantPeople(true); // ✅ Force cache bypass
}
```

## Files Modified

### Backend
1. ✅ `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs`
   - Added `invalidateCache` parameter to both endpoints
   - Updated logging to include cache flag

2. ✅ `UNOPS.PAO.Business/Interfaces/IGeminiManager.cs`
   - Updated interface method signatures

3. ✅ `UNOPS.PAO.Business/Managers/GeminiManager.cs`
   - Updated base implementation signatures

4. ✅ `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs`
   - Implemented cache bypass in both methods
   - Pass `bypassCache` to `FetchResultFromGemini` calls

### Frontend
5. ✅ `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/services/opportunity.service.ts`
   - Added `invalidateCache` parameter to both service methods
   - Pass to API as query parameter

6. ✅ `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.ts`
   - Updated `loadSimilarProjects` and `loadRelevantPeople` to accept parameter
   - Updated `refreshSimilarProjects` and `refreshRelevantPeople` to pass `true`

## Usage

### User Perspective

When viewing the DST section of an opportunity:

1. **Initial Load**: Uses cached results if available (faster response)
2. **Click Refresh Icon**: Forces fresh data fetch, bypassing cache
3. **Result**: Always get most up-to-date information from vector store and AI

### Developer Perspective

#### Testing Without Cache
```typescript
// Force fresh data during testing
this.loadSimilarProjects(true);
this.loadRelevantPeople(true);
```

#### API Calls
```bash
# Normal call (uses cache)
GET /api/opportunities/123/similar-projects?maxResults=6

# Force refresh (bypasses cache)
GET /api/opportunities/123/similar-projects?maxResults=6&invalidateCache=true
```

## Benefits

✅ **Testing**: Can now test vector store and validation fixes without cache interference  
✅ **Development**: Easier to verify AI prompt changes and data flow  
✅ **Production**: Users can force refresh when needed  
✅ **Performance**: Default behavior still uses cache for fast responses  
✅ **Flexibility**: Cache invalidation available but not required  

## Backward Compatibility

✅ **Parameter is optional** with default `false` value  
✅ **Existing calls work unchanged** - cache behavior maintained  
✅ **No breaking changes** to API contracts  

## Testing Instructions

### 1. Test Cached Behavior (Default)
- Navigate to opportunity DST section
- Click "Load Similar Projects"
- Note response time
- Click again - should be instant (cached)

### 2. Test Cache Invalidation
- Click refresh icon (♻️) on Similar Projects card
- Backend logs should show: `invalidateCache=True`
- Fresh AI call should be made
- Response may be slower (no cache)

### 3. Verify Fresh Data
- Modify opportunity details that affect search
- Click refresh icon
- Verify results reflect updated opportunity data

## Backend Logging

When `invalidateCache=true`, logs will show:

```
🔍 [SIMILAR-PROJECTS] Starting similar projects search for opportunity 123, invalidateCache=True
✅ [SIMILAR-PROJECTS] Vector store search returned 5 results
🤖 [SIMILAR-PROJECTS] Refining 5 projects with AI-generated relevance explanations
✅ [SIMILAR-PROJECTS] Successfully added relevance explanations to 5 projects
```

The AI service will bypass its internal cache and make a fresh Gemini API call.

## Related Features

- Vector Store Integration (BigQuery semantic search)
- AI Refinement (Gemini adds relevance explanations)
- Error Handling & Validation (prevents hallucination)
- ID Preservation (maintains data integrity)

## Status

✅ **Implemented**: Cache invalidation parameter added throughout stack  
✅ **Tested**: No linter errors in all modified files  
⏳ **Pending**: Runtime testing with actual vector store data  

## Next Steps

1. **Rebuild Backend**: Recompile with new parameter support
2. **Test Cache Behavior**: Verify cache is used by default
3. **Test Invalidation**: Verify refresh icon bypasses cache
4. **Monitor Logs**: Check that invalidateCache flag propagates correctly
5. **Verify Fresh Data**: Confirm non-cached results reflect latest data

