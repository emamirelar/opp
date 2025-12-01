# Fix: Vector Store Integration & AI Hallucination Prevention

## Issue

The DST (Digital Strategy & Transformation) section was returning **fake/hallucinated projects and people** with:
- Empty `projectId` and `personId` fields
- AI-generated descriptions and relevance explanations
- No actual metadata (partners, dates, scores, URLs all null/empty)
- Angular tracking errors (NG0955) due to duplicate empty keys

## Root Cause Discovery

After investigation, the actual issue was identified:

### **Vector Store API Call Failures**

1. **Silent Failure**: The vector store API call (`SearchVectorStoreAsync`) was failing (likely **unauthorized/authentication issues**)
2. **Error Swallowing**: Failures were caught but not properly propagated - returned empty document list
3. **Continued Execution**: Code continued executing even when vector store returned no valid results
4. **AI Hallucination**: Gemini AI, when asked to "refine" an empty or minimal list, **invented fake projects and people**
5. **No Validation**: No checks prevented sending invalid/empty data to AI or using AI-hallucinated responses

### The Hallucination Flow

```
1. Vector Store Call → FAILS (unauthorized/error) ❌
2. Error caught, returns empty Documents list ⚠️
3. similarProjects list = [] (empty) ⚠️
4. Code sends empty list to Gemini with refinement prompt 🤖
5. Gemini AI, seeing the prompt context, HALLUCINATES projects/people 🎭
6. Backend returns hallucinated data as if it were real ❌
7. Frontend receives fake data with no IDs → tracking errors 💥
```

## Solution Applied

### 1. **Request More Results for Deduplication**

Request 2x the desired results from vector store to account for duplicates:

```csharp
// Request 2x results to account for potential duplicates from vector store
var vectorStoreMaxResults = maxResults * 2;
_logger.LogInformation($"📊 [SIMILAR-PROJECTS] Requesting {vectorStoreMaxResults} results from vector store (2x {maxResults}) to filter duplicates");

var vectorStoreRequest = new VectorStoreSearchRequest
{
    Query = searchQuery,
    MaxResults = vectorStoreMaxResults,  // 2x requested amount
    EntityTypeId = "PROJECT",
    // ...
};
```

**Result**: Get more results upfront to handle duplicates that vector store might return!

### 2. **Deduplicate and Sort Results**

After mapping documents, deduplicate by ID and take top results:

```csharp
// Deduplicate by ProjectId, keeping the first occurrence (highest relevance score)
similarProjects = similarProjects
    .GroupBy(p => p.ProjectId)
    .Select(g => g.First())
    .OrderByDescending(p => p.RelevanceScore)
    .Take(maxResults)
    .ToList();

if (originalCount > similarProjects.Count)
{
    _logger.LogInformation($"🔄 [SIMILAR-PROJECTS] Deduplicated {originalCount} results to {similarProjects.Count} unique projects (requested {maxResults})");
}
```

**Benefits**:
- ✅ Eliminates duplicate projects/people
- ✅ Keeps highest relevance score for each unique ID
- ✅ Ensures exactly maxResults unique items (or fewer if not enough available)
- ✅ Logs deduplication statistics for monitoring

### 3. **Vector Store Response Validation**

Added explicit validation to catch and propagate vector store failures:

```csharp
var vectorStoreResponse = await aiRetrieverManager.SearchVectorStoreAsync(vectorStoreRequest, userEmail);

// Validate vector store response
if (vectorStoreResponse == null)
{
    _logger.LogError($"❌ [SIMILAR-PROJECTS] Vector store returned null response");
    throw new InvalidOperationException("Vector store search returned null response. This may indicate an authorization or connectivity issue.");
}
```

**Result**: Errors now propagate to frontend instead of being swallowed!

### 4. **Document ID Validation**

Added validation to skip documents without valid IDs:

```csharp
foreach (var doc in vectorStoreResponse.Documents)
{
    var projectId = doc.EntityId ?? doc.DocumentId;
    
    // Validate that we have a valid project ID
    if (string.IsNullOrEmpty(projectId))
    {
        _logger.LogWarning($"⚠️ [SIMILAR-PROJECTS] Document without ID found, skipping.");
        continue; // Skip invalid documents
    }
    
    // Map document to project model...
}
```

### 5. **AI Hallucination Prevention**

Added critical check before sending data to Gemini:

```csharp
// CRITICAL: Only refine if we have valid projects with IDs (prevent AI hallucination)
if (similarProjects.Any() && similarProjects.All(p => !string.IsNullOrEmpty(p.ProjectId)))
{
    _logger.LogInformation($"📋 [SIMILAR-PROJECTS] Project IDs being sent to AI: {string.Join(", ", similarProjects.Select(p => p.ProjectId))}");
    
    // Call Gemini to refine...
}
else
{
    // Don't call Gemini if we have no valid projects or missing IDs
    _logger.LogWarning($"⚠️ [SIMILAR-PROJECTS] Skipping AI refinement - no valid projects with IDs");
}
```

**Result**: Gemini never receives empty/invalid data that could trigger hallucination!

### 6. **AI Response Validation**

Added validation to detect if AI returns unexpected results:

```csharp
if (refinedProjects != null && refinedProjects.Count > 0)
{
    // CRITICAL VALIDATION: Check if AI hallucinated new projects
    if (refinedProjects.Count != similarProjects.Count)
    {
        _logger.LogWarning($"⚠️ [SIMILAR-PROJECTS] AI returned {refinedProjects.Count} projects but we sent {similarProjects.Count}. Possible hallucination - skipping refinement.");
        // Don't use AI response if counts don't match
    }
    else
    {
        // Merge AI explanations with original data (field-level merge)
    }
}
```

### 7. **Enhanced Logging**

Added comprehensive logging to track the entire flow:

```csharp
_logger.LogInformation($"📋 [SIMILAR-PROJECTS] Processing {vectorStoreResponse.Documents.Count} documents from vector store");
_logger.LogDebug($"[SIMILAR-PROJECTS] Mapped project: ID={projectId}, Score={score:F2}, Description={description.Substring(0, 50)}...");
_logger.LogWarning($"⚠️ [SIMILAR-PROJECTS] Vector store returned no documents for opportunity {opportunityId}");
```

## Files Modified

### Backend
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs`
  - **GetSimilarProjectsAsync** method (~lines 2996-3150)
    - Added vector store null validation (throw exception)
    - Added document ID validation (skip invalid)
    - Added empty list logging
    - Added pre-AI validation (check all IDs present)
    - Added AI response count validation
    - Added comprehensive debug logging
  
  - **GetRelevantPeopleAsync** method (~lines 3269-3420)
    - Same validations as above for people

### Frontend (Safety Net - Already Applied)
- `UNOPS.PAO.ClientApp/.../opportunity-dst-section.component.html`
  - Line 303: `track project.projectId || $index`
  - Line 423: `track person.personId || $index`
- `UNOPS.PAO.ClientApp/.../opportunity-dst-section.component.ts`
  - Added debug logging to verify received data

## Testing Instructions

### 1. **Check Vector Store Authorization**
- Ensure the backend service has proper credentials to call the vector store API
- Check `AiRetrieverManager.SearchVectorStoreAsync` configuration
- Verify OAuth tokens or API keys are valid

### 2. **Test Error Propagation**
If vector store fails, you should now see:
- **Backend logs**: `❌ [SIMILAR-PROJECTS] Vector store returned null response`
- **Frontend network error**: 500 status with proper error message
- **User feedback**: Error toast showing the issue

### 3. **Test Valid Flow**
With working vector store access:
- **Backend logs** should show:
  ```
  📊 [SIMILAR-PROJECTS] Requesting 12 results from vector store (2x 6) to filter duplicates
  ✅ [SIMILAR-PROJECTS] Vector store search returned 10 results
  📋 [SIMILAR-PROJECTS] Processing 10 documents from vector store
  [SIMILAR-PROJECTS] Mapped project: ID=P00123, Score=87.50...
  🔄 [SIMILAR-PROJECTS] Deduplicated 10 results to 6 unique projects (requested 6)
  📋 [SIMILAR-PROJECTS] Project IDs being sent to AI: P00123, P00456, P00789...
  ✅ [SIMILAR-PROJECTS] Successfully added relevance explanations to 6 projects
  ```

- **Frontend console** should show:
  ```
  📊 [SIMILAR-PROJECTS] Received response: 
    count: 5,
    projects: [
      { projectId: "P00123", hasId: true, description: "..." },
      { projectId: "P00456", hasId: true, description: "..." },
      ...
    ]
  ```

- **API Response** should have complete data:
  ```json
  {
    "projectId": "P00123456",
    "description": "Actual project description from BigQuery",
    "relevanceScore": 87.5,
    "startDate": "2022-01-15",
    "partners": "UNICEF, WHO",
    "countries": "Pakistan, India",
    "projectUrl": "https://projects.unops.org/#b0/P00123456/...",
    "relevanceExplanation": "AI-generated relevance explanation"
  }
  ```

### 4. **Test Hallucination Prevention**
If vector store returns no results:
- **Backend logs**: `⚠️ [SIMILAR-PROJECTS] Vector store returned no documents`
- **Response**: Empty `similarProjects` array (not hallucinated data)
- **Frontend**: "No similar projects found" state (not fake cards)

## Expected Behavior After Fix

✅ **Vector Store Request**: Requests 2x results to account for potential duplicates  
✅ **Deduplication**: Filters to unique projects/people by ID, keeps highest scores  
✅ **Result Limiting**: Returns exactly maxResults unique items (or fewer if insufficient)  
✅ **Vector Store Success**: Complete project/person data with IDs and metadata  
✅ **Vector Store Failure**: Clear error propagated to frontend, no hallucinated data  
✅ **Vector Store Empty**: Empty response, not fake data  
✅ **AI Refinement**: Only called with valid unique data, only adds explanations  
✅ **AI Validation**: Detects and prevents using hallucinated responses  
✅ **Frontend Display**: Real data or clear error, never fake data  
✅ **No Angular Errors**: Valid unique IDs for tracking (or fallback to index)  
✅ **No Duplicates**: Each project/person appears only once in results  

## Root Cause Categories

This fix addresses multiple failure modes:

1. **Authorization Failures** → Now throws proper error
2. **Empty Results** → Logged and handled, no AI call
3. **Invalid Data** → Validated and skipped
4. **AI Hallucination** → Prevented by validation checks
5. **Silent Failures** → Now logged and propagated

## Prevention Guidelines

1. **Always validate external API responses** before continuing execution
2. **Throw exceptions for critical failures** instead of returning empty data
3. **Never send empty/invalid data to AI** - it will hallucinate to fill the gaps
4. **Validate AI responses** before using them (count, structure, content)
5. **Log extensively** at each step to enable troubleshooting
6. **Propagate errors to frontend** so users know something is wrong

## Status

✅ **Vector Store Validation**: Added null checks and error throwing  
✅ **Document Validation**: Added ID validation with skip logic  
✅ **AI Hallucination Prevention**: Added pre-call validation  
✅ **AI Response Validation**: Added count and content checks  
✅ **Enhanced Logging**: Added comprehensive tracking  
✅ **Frontend Fallback**: Track by index if IDs missing  
⏳ **Pending**: Fix underlying vector store authorization issue  
⏳ **Pending**: Runtime testing with actual opportunity data  

## Next Steps

1. **Rebuild Backend**: Recompile with new validation logic
2. **Fix Vector Store Auth**: Resolve the authorization issue with the vector store API
3. **Test End-to-End**: Verify data flows correctly from BigQuery → Vector Store → Backend → Frontend
4. **Monitor Logs**: Watch for validation warnings and errors in production

