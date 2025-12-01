# Fix: UNOPS Relevant Experience Tracking Key Errors

## Issue

The DST (Digital Strategy & Transformation) section was throwing Angular tracking errors:

```
NG0955: The provided track expression resulted in duplicated keys for a given collection.
Duplicated keys were: key "" at index "0" and "1", key "" at index "1" and "2", ...
```

This occurred in two locations:
- Line 303: Similar Projects grid (`track project.projectId`)
- Line 423: Relevant People grid (`track person.personId`)

## Root Cause

The backend AI refinement process was **completely replacing** original objects with AI-generated partial objects:

1. **Initial Data Creation** ✅: The backend correctly created complete objects with all metadata:
   - `ProjectId` from `doc.EntityId ?? doc.DocumentId`
   - `RelevanceScore` calculated from vector similarity
   - All metadata fields (StartDate, Partners, Countries, URLs, etc.)

2. **AI Refinement** ✅: Original objects sent to Gemini AI to add relevance explanations:
   - Complete objects serialized to JSON
   - Sent to AI with instruction to add `relevanceExplanation` field

3. **AI Response Problem** ❌: Gemini returned **partial objects** with only selected fields:
   - Only returned: `description` and `relevanceExplanation`
   - Did NOT return: `projectId`, `relevanceScore`, or any metadata
   - All omitted fields defaulted to empty/null in deserialization

4. **Object Replacement Bug** ❌: Backend **replaced** original objects with AI partial objects:
   - Original: `similarProjects = refinedProjects;`
   - Result: Lost all IDs and metadata, kept only AI-returned fields
   - Final API response had empty IDs and null metadata

5. **Frontend Impact**: Angular received objects with empty string IDs:
   - Multiple objects had `projectId: ""` and `personId: ""`
   - `@for` tracking expression found duplicate empty keys
   - Angular threw NG0955 errors

## Solution

### Backend Changes (UNOPSGeminiManager.cs)

Applied two fixes to both `GetSimilarProjectsAsync()` and `GetRelevantPeopleAsync()` methods:

#### 1. Field-Level Merging Instead of Object Replacement
**Changed strategy**: Instead of replacing original objects with AI response, now we **merge at field level**:

```csharp
var refinedProjects = JsonConvert.DeserializeObject<List<SimilarProjectModel>>(
    refinedData["projects"].ToString(), 
    deserializationSettings
);

if (refinedProjects != null)
{
    // IMPORTANT: AI only returns relevanceExplanation - merge with original data
    for (int i = 0; i < Math.Min(similarProjects.Count, refinedProjects.Count); i++)
    {
        var originalProject = similarProjects[i];
        var refinedProject = refinedProjects[i];
        
        // Only update relevanceExplanation from AI response
        // Preserve ALL other original fields (ID, metadata, scores, etc.)
        if (!string.IsNullOrEmpty(refinedProject.RelevanceExplanation))
        {
            originalProject.RelevanceExplanation = refinedProject.RelevanceExplanation;
        }
    }
    // Keep original list (don't replace with AI response)
}
```

**Key Changes**:
- ❌ **Before**: `similarProjects = refinedProjects;` (replaced entire objects)
- ✅ **After**: `originalProject.RelevanceExplanation = refinedProject.RelevanceExplanation;` (merge single field)
- ✅ **Result**: Preserves all IDs, metadata, scores from original vector search results

#### 2. Debug Logging for Verification
Added detailed logging to track ID preservation:

```csharp
_logger.LogDebug($"[SIMILAR-PROJECTS] Project {i}: ID={originalProject.ProjectId}, HasExplanation={!string.IsNullOrEmpty(originalProject.RelevanceExplanation)}");
```

**Benefits**:
- Verifies each project retains its original ID
- Confirms relevance explanations are successfully added
- Helps troubleshoot any future AI response issues

## Files Modified

### Backend
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs`
  - Lines ~3069-3090: Similar Projects deserialization fix
  - Lines ~3322-3343: Relevant People deserialization fix

### Frontend
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.html`
  - Line 303: `track project.projectId || $index` (fallback to index if ID is empty)
  - Line 423: `track person.personId || $index` (fallback to index if ID is empty)
  
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.ts`
  - Added debug logging to verify IDs are being received from backend
  - Logs project/person IDs in console for troubleshooting

## Testing Instructions

1. **Navigate to Opportunity DST Section**:
   - Open any opportunity in the system
   - Go to the "DST Insights & Recommendations" tab

2. **Load Similar Projects**:
   - Click "Load Similar Projects" button
   - Verify projects display without console errors
   - Check that each project card shows proper data
   - Verify clicking on projects opens correct URLs

3. **Load Relevant People**:
   - Click "Load Relevant People" button
   - Verify people display without console errors
   - Check that each person card shows proper data
   - Verify profile photos and contact information display

4. **Console Verification**:
   - Open browser console (F12)
   - Verify no NG0955 errors appear
   - Look for debug logs showing received data:
     - `📊 [SIMILAR-PROJECTS] Received response:` with project IDs
     - `👥 [RELEVANT-PEOPLE] Received response:` with person IDs
   - Verify all `hasId` values are `true`
   - Check backend logs for successful ID preservation messages

## Expected Behavior After Fix

✅ **Vector Search Results**: Complete objects with IDs, scores, and metadata from BigQuery
✅ **AI Refinement**: Only adds `relevanceExplanation` field, all other fields preserved
✅ **Backend Response**: Full objects with IDs + metadata + AI explanations
✅ **Frontend Reception**: Angular receives objects with unique, valid IDs
✅ **Tracking Expression**: `@for` loop tracks items by unique IDs (with `$index` fallback)
✅ **User Experience**: Grids display with all data + no console errors

**Sample Response After Fix**:
```json
{
  "projectId": "P12345",
  "description": "Water governance project...",
  "relevanceScore": 87.5,
  "startDate": "2022-01-15",
  "partners": "UNICEF, WHO",
  "countries": "Pakistan, India",
  "projectUrl": "https://projects.unops.org/#b0/P12345/...",
  "relevanceExplanation": "Similar focus on water governance..."
}
```

## Related Components

- **AI Services**: Gemini AI refinement prompts
  - `opportunity_refine_projects` prompt
  - `opportunity_refine_people` prompt
- **Vector Store**: BigQuery-based semantic search
- **Entity Types**: PROJECT and PERSON entities
- **Frontend Models**: TypeScript interfaces in `opportunity.model.ts`

## Prevention

To prevent similar issues in the future:

1. **Never replace objects wholesale** - Always merge at field level when updating from AI responses
2. **Preserve original data** - AI should augment, not replace, vector search results
3. **Use field-level updates** - Only update specific fields that AI is meant to provide
4. **Add verification logging** - Log IDs before/after AI processing to catch data loss
5. **Test end-to-end** - Verify actual API responses include all expected fields
6. **AI Prompt Guidelines** - Clearly instruct AI on which fields to return (or use merging strategy regardless)

## Status

✅ **Fixed**: Backend deserialization now handles camelCase from AI responses
✅ **Tested**: No linter errors in modified backend files
⏳ **Pending**: Runtime testing with actual opportunity data

