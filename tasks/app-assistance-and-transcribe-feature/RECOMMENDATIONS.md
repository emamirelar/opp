# AI Section Assistance Feature - Architecture Recommendations

**Date:** November 26, 2025  
**Version:** 1.0  
**Status:** Draft for Review

---

## Executive Summary

This document provides comprehensive recommendations for implementing a contextual AI assistance feature that will be embedded in section headers throughout the application. The feature will allow users to:

- Ask questions about any section or the application
- Upload documents for context
- Auto-fill form fields based on AI-generated content
- Receive intelligent, context-aware responses

The implementation leverages existing infrastructure (Gemini AI, document handling, prompt management) while introducing new components for guidance storage, retrieval, and contextual processing.

---

## Table of Contents

1. [Current System Analysis](#current-system-analysis)
2. [Storage Strategy for Application Guidance](#storage-strategy-for-application-guidance)
3. [Retrieval Strategy: Embeddings vs Direct Retrieval](#retrieval-strategy-embeddings-vs-direct-retrieval)
4. [Architecture Overview](#architecture-overview)
5. [Component Design](#component-design)
6. [Backend API Design](#backend-api-design)
7. [Data Flow Diagrams](#data-flow-diagrams)
8. [Implementation Phases](#implementation-phases)
9. [Security and Performance Considerations](#security-and-performance-considerations)
10. [Next Steps](#next-steps)

---

## 1. Current System Analysis

### 1.1 Existing Infrastructure

**✅ Components We Can Leverage:**

1. **AiPrompt Table** (`public.AiPrompt`)
   - Already supports multiple prompt types with system instructions
   - Has `DataRetrievalMethod` field for dynamic data fetching
   - Supports caching (`UseCache`, `CacheInvalidationMinutes`)
   - Includes `Feature` field for categorization

2. **Document Handling System**
   - Google Cloud Storage (GCS) integration via `GoogleCloudStorageService`
   - Supports `gs://` URIs for Gemini document processing
   - Document upload with `UploadToGCS` and `SkipDatabaseSave` flags
   - MIME type handling for various file types

3. **AI Processing Infrastructure**
   - `AiContextualService` with placeholder processing
   - `UNOPSGeminiManager` with comprehensive Gemini API integration
   - Support for documents in prompts (via `fileData` and `fileUri`)
   - Caching via `AiPromptCacheService`

4. **Vector Store Integration**
   - `search_corp_vector_store` tool in AI assistant
   - Support for entityTypeId filtering (including "GUIDANCE")
   - Already integrated with Python AI service

5. **Existing UI Components**
   - `AiTranscribeComponent` for file upload and processing
   - `AiPanelComponent` for displaying AI-generated content
   - Document upload functionality in various components

### 1.2 Gaps to Address

**❌ Missing Components:**

1. **Guidance Storage**: No dedicated table or mechanism for storing application guidance markdown
2. **Section Context Management**: No system to identify which guidance applies to which sections
3. **Shared AI Popup Component**: No reusable component for contextual AI assistance
4. **Guidance Retrieval API**: No endpoint to fetch relevant guidance for a specific context
5. **Field Population Logic**: No mechanism to map AI responses to form fields in arbitrary sections

---

## 2. Storage Strategy for Application Guidance

### 2.1 Recommendation: New `ApplicationGuidance` Table

**✅ Create a dedicated table for storing guidance content**

#### Table Schema

```sql
CREATE TABLE public."ApplicationGuidance" (
    "Id" SERIAL PRIMARY KEY,
    
    -- Core identification
    "Title" VARCHAR(500) NOT NULL,
    "Slug" VARCHAR(255) UNIQUE NOT NULL, -- URL-friendly identifier
    "GuidanceType" VARCHAR(100) NOT NULL, -- 'section', 'feature', 'general', 'workflow'
    
    -- Context mapping
    "Feature" VARCHAR(100), -- 'opportunities', 'partnerships', 'interactions', etc.
    "Section" VARCHAR(100), -- 'statement', 'details', 'budget', etc.
    "ComponentPath" TEXT, -- Angular component path for precise matching
    "Route" VARCHAR(500), -- URL route pattern
    
    -- Content
    "ContentMarkdown" TEXT NOT NULL, -- The actual guidance in markdown
    "Summary" TEXT, -- Brief summary for search/display
    "Keywords" TEXT[], -- Array of keywords for matching
    
    -- Metadata
    "Category" VARCHAR(100), -- 'how-to', 'explanation', 'reference', 'troubleshooting'
    "Priority" INTEGER DEFAULT 50, -- For ranking search results (0-100)
    "IsActive" BOOLEAN DEFAULT TRUE,
    "Version" VARCHAR(50), -- Track guidance versions
    
    -- Embeddings (optional - see Section 3)
    "HasEmbedding" BOOLEAN DEFAULT FALSE,
    "EmbeddingLastUpdated" TIMESTAMP WITH TIME ZONE,
    
    -- Audit
    "CreatedBy" INTEGER NOT NULL,
    "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastModifiedBy" INTEGER,
    "LastModifiedDate" TIMESTAMP WITH TIME ZONE,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "DeletedBy" INTEGER,
    "DeletedDate" TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT "FK_ApplicationGuidance_CreatedBy" FOREIGN KEY ("CreatedBy") 
        REFERENCES "AspNetUsers"("Id"),
    CONSTRAINT "FK_ApplicationGuidance_LastModifiedBy" FOREIGN KEY ("LastModifiedBy") 
        REFERENCES "AspNetUsers"("Id"),
    CONSTRAINT "FK_ApplicationGuidance_DeletedBy" FOREIGN KEY ("DeletedBy") 
        REFERENCES "AspNetUsers"("Id")
);

-- Indexes for performance
CREATE INDEX "IX_ApplicationGuidance_Feature_Section" 
    ON public."ApplicationGuidance"("Feature", "Section") WHERE "IsActive" = TRUE AND "IsDeleted" = FALSE;

CREATE INDEX "IX_ApplicationGuidance_GuidanceType" 
    ON public."ApplicationGuidance"("GuidanceType") WHERE "IsActive" = TRUE AND "IsDeleted" = FALSE;

CREATE INDEX "IX_ApplicationGuidance_Keywords" 
    ON public."ApplicationGuidance" USING GIN("Keywords") WHERE "IsActive" = TRUE AND "IsDeleted" = FALSE;

CREATE INDEX "IX_ApplicationGuidance_Route" 
    ON public."ApplicationGuidance"("Route") WHERE "IsActive" = TRUE AND "IsDeleted" = FALSE;
```

#### Entity Class (.NET)

```csharp
namespace UNOPS.PAO.Domain.Entities;

public class ApplicationGuidance : ModifiableDeletableEntity<int, int>
{
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string GuidanceType { get; set; } // section, feature, general, workflow
    
    // Context mapping
    public string? Feature { get; set; }
    public string? Section { get; set; }
    public string? ComponentPath { get; set; }
    public string? Route { get; set; }
    
    // Content
    public required string ContentMarkdown { get; set; }
    public string? Summary { get; set; }
    public string[]? Keywords { get; set; }
    
    // Metadata
    public string? Category { get; set; }
    public int Priority { get; set; } = 50;
    public bool IsActive { get; set; } = true;
    public string? Version { get; set; }
    
    // Embeddings (optional)
    public bool HasEmbedding { get; set; } = false;
    public DateTime? EmbeddingLastUpdated { get; set; }
}
```

### 2.2 Alternative: Extend AiPrompt Table

**❌ Not Recommended** - Here's why:

**Cons:**
- AiPrompt is designed for AI processing configurations, not content storage
- Mixing guidance content with prompt configurations reduces clarity
- Different access patterns (guidance is content-focused, prompts are config-focused)
- Would require significant schema changes
- Harder to manage and version guidance independently

**Pros:**
- Fewer tables
- Reuse existing caching infrastructure

**Verdict:** Create a dedicated `ApplicationGuidance` table for better separation of concerns.

### 2.3 Alternative: Store in Files/Documents

**❌ Not Recommended** - Here's why:

**Cons:**
- No database querying capabilities (filtering, searching, joining)
- Harder to version and track changes
- No referential integrity with other entities
- Requires file system access for retrieval
- Complicates permissions and access control

**Pros:**
- Easy to edit markdown files directly
- Version control via Git

**Verdict:** Database storage is superior for this use case.

---

## 3. Retrieval Strategy: Embeddings vs Direct Retrieval

### 3.1 Recommendation: Hybrid Approach

**✅ Use both direct retrieval AND embeddings for optimal results**

#### Phase 1: Direct Retrieval (MVP)

For the initial implementation, use **direct database queries** to retrieve relevant guidance:

**Query Logic:**
```sql
SELECT * FROM public."ApplicationGuidance"
WHERE "IsActive" = TRUE 
    AND "IsDeleted" = FALSE
    AND (
        -- Exact context match (highest priority)
        ("Feature" = @feature AND "Section" = @section)
        OR 
        -- Feature-level match
        ("Feature" = @feature AND "Section" IS NULL)
        OR
        -- Route pattern match
        @currentRoute LIKE "Route" || '%'
        OR
        -- Keyword match
        "Keywords" && ARRAY[@keyword1, @keyword2, ...]
        OR
        -- General guidance
        "GuidanceType" = 'general'
    )
ORDER BY 
    CASE 
        WHEN "Feature" = @feature AND "Section" = @section THEN 1
        WHEN "Feature" = @feature AND "Section" IS NULL THEN 2
        WHEN @currentRoute LIKE "Route" || '%' THEN 3
        WHEN "Keywords" && ARRAY[@keyword1, @keyword2, ...] THEN 4
        ELSE 5
    END,
    "Priority" DESC
LIMIT 5;
```

**Advantages:**
- ✅ Simple to implement
- ✅ No ML infrastructure needed initially
- ✅ Fast for exact matches
- ✅ Deterministic results
- ✅ Easy to debug and understand

**Disadvantages:**
- ❌ Limited semantic understanding
- ❌ Requires exact keyword matches
- ❌ No similarity scoring for content
- ❌ Can't handle synonyms or related concepts

#### Phase 2: Add Embeddings (Enhancement)

After MVP, integrate with the existing vector store:

**Implementation:**
1. Generate embeddings for `ContentMarkdown` using Vertex AI
2. Store embeddings in existing `EntityEmbeddings` table (extend for guidance)
3. Use hybrid retrieval: direct match + semantic search
4. Leverage existing `search_corp_vector_store` with `entityTypeId: "GUIDANCE"`

**Query Logic (Hybrid):**
```csharp
// 1. Get direct matches (fast, exact)
var directMatches = await GetDirectGuidanceMatches(feature, section, route);

// 2. Get semantic matches from vector store
var semanticMatches = await SearchVectorStore(
    query: userPrompt,
    entityTypeId: "GUIDANCE",
    maxResults: 5
);

// 3. Merge and rank results
var combinedResults = MergeAndRankResults(directMatches, semanticMatches);

// 4. Return top N results
return combinedResults.Take(5);
```

**Advantages:**
- ✅ Semantic understanding of user questions
- ✅ Handles synonyms and related concepts
- ✅ Better for exploratory questions
- ✅ Leverages existing vector infrastructure

**Disadvantages:**
- ❌ More complex implementation
- ❌ Requires embedding generation pipeline
- ❌ Additional storage costs
- ❌ Needs periodic re-indexing

### 3.2 Embedding Strategy (Phase 2 Detail)

#### Option A: Extend EntityEmbeddings Table

```sql
-- Add guidance support to existing EntityEmbeddings
INSERT INTO public."EntityEmbeddings" (
    "EntityType",
    "EntityId",
    "Embedding",
    "Model",
    "CreatedDate"
)
VALUES (
    'ApplicationGuidance',
    <guidance_id>,
    <embedding_vector>,
    'textembedding-gecko@003',
    NOW()
);
```

#### Option B: Dedicated GuidanceEmbeddings Table

```sql
CREATE TABLE public."GuidanceEmbeddings" (
    "Id" SERIAL PRIMARY KEY,
    "GuidanceId" INTEGER NOT NULL REFERENCES public."ApplicationGuidance"("Id") ON DELETE CASCADE,
    "Embedding" vector(768), -- pgvector extension
    "Model" VARCHAR(100) NOT NULL,
    "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastUpdated" TIMESTAMP WITH TIME ZONE
);

CREATE INDEX "IX_GuidanceEmbeddings_Embedding" 
    ON public."GuidanceEmbeddings" USING ivfflat ("Embedding" vector_cosine_ops);
```

**Recommendation:** Use **Option A (extend EntityEmbeddings)** to leverage existing infrastructure.

### 3.3 Recommended Approach Summary

| Phase | Strategy | Timeline | Complexity |
|-------|----------|----------|------------|
| **Phase 1 (MVP)** | Direct DB retrieval with context matching | Sprint 1-2 | Low |
| **Phase 2 (Enhancement)** | Hybrid: Direct + Vector embeddings | Sprint 3-4 | Medium |
| **Phase 3 (Optimization)** | Fine-tune ranking algorithms, caching | Sprint 5+ | Medium |

---

## 4. Architecture Overview

### 4.1 System Components

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend (Angular)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Section Header (any component)                            │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  [AI Icon] → Opens AI Assistance Popup             │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                          ↓                                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ AiSectionAssistanceComponent (new shared component)      │  │
│  │  • Prompt input box                                      │  │
│  │  • Document attachment (reuse document upload)           │  │
│  │  • Quick action buttons ("Explain this section", etc.)   │  │
│  │  • Response display area                                 │  │
│  │  • Auto-fill capability                                  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                          ↓                                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Services                                                  │  │
│  │  • AiSectionAssistanceService (new)                      │  │
│  │  • DocumentService (existing)                            │  │
│  │  • GeminiService (existing)                              │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                          ↓ HTTP API Calls
┌─────────────────────────────────────────────────────────────────┐
│                        Backend (.NET)                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ AiSectionAssistanceController (new)                      │  │
│  │  • POST /ai/section-assistance                           │  │
│  │  • GET /ai/section-guidance                              │  │
│  │  • POST /ai/section-assistance/autofill                  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                          ↓                                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Business Layer                                            │  │
│  │  • AiSectionAssistanceManager (new)                      │  │
│  │  • ApplicationGuidanceManager (new)                      │  │
│  │  • UNOPSGeminiManager (existing - extend)                │  │
│  │  • DocumentManager (existing)                            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                          ↓                                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Data Access                                               │  │
│  │  • ApplicationGuidance (new table)                       │  │
│  │  • AiPrompt (existing - new prompt types)                │  │
│  │  • Documents (existing)                                   │  │
│  │  • EntityEmbeddings (existing - extend for guidance)     │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│                    External Services                             │
├─────────────────────────────────────────────────────────────────┤
│  • Gemini 2.0 API (Vertex AI)                                   │
│  • Google Cloud Storage (document storage)                      │
│  • Python AI Service (vector store search - optional Phase 2)   │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Data Flow: User Question Processing

```
┌──────────────┐
│ User clicks  │
│  AI icon in  │
│   section    │
└──────┬───────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 1. AI Popup Opens                                            │
│    • Auto-loads section context (feature, section, route)   │
│    • Shows quick action buttons                             │
│    • User types question OR uploads document                │
└──────┬──────────────────────────────────────────────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Frontend Service Call                                     │
│    POST /api/ai/section-assistance                          │
│    {                                                         │
│      "prompt": "What should I include in the budget?",      │
│      "feature": "opportunities",                            │
│      "section": "budget",                                   │
│      "currentRoute": "/opportunities/123/edit",             │
│      "entityId": 123,                                       │
│      "documentIds": [45, 67],  // Optional                  │
│      "intent": "question"  // question, autofill, explain   │
│    }                                                         │
└──────┬──────────────────────────────────────────────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Backend Processing                                        │
│    A. Fetch relevant guidance                               │
│       • Query ApplicationGuidance table                     │
│       • Match on feature + section                          │
│       • Get top 3-5 guidance documents                      │
│                                                              │
│    B. Fetch entity data (if needed)                         │
│       • Load opportunity/partner/interaction details        │
│       • Get current field values                            │
│                                                              │
│    C. Fetch document content (if provided)                  │
│       • Get gs:// URIs from document IDs                    │
│       • Include in Gemini context                           │
└──────┬──────────────────────────────────────────────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Build Gemini Prompt                                       │
│    System Instructions:                                      │
│    • You are an expert UNOPS assistant                      │
│    • Context: User is on [feature]/[section]                │
│    • Here is relevant guidance: [guidance markdown]         │
│    • Current entity data: [entity JSON]                     │
│    • Intent: [question/autofill/explain]                    │
│                                                              │
│    User Prompt:                                              │
│    • User question: [prompt]                                │
│    • Documents: [gs:// URIs]                                │
└──────┬──────────────────────────────────────────────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Call Gemini API                                           │
│    • Send prompt + documents to Gemini 2.0                  │
│    • Get response (markdown or JSON)                        │
└──────┬──────────────────────────────────────────────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Process Response                                          │
│    If intent = "question":                                  │
│      • Return markdown response to frontend                 │
│                                                              │
│    If intent = "autofill":                                  │
│      • Parse JSON field mappings                            │
│      • Validate field names against entity                  │
│      • Return field-value pairs                             │
│                                                              │
│    If intent = "explain":                                   │
│      • Return explanation with guidance links               │
└──────┬──────────────────────────────────────────────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. Frontend Displays Response                                │
│    • Show markdown in response area                         │
│    • If autofill: populate form fields with animation       │
│    • Show "Accept" / "Reject" for autofill suggestions      │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Component Design

### 5.1 Frontend: AiSectionAssistanceComponent

**Purpose:** Reusable Angular component for contextual AI assistance

**Inputs:**
```typescript
// Core context
@Input() feature: string;           // 'opportunities', 'partnerships', etc.
@Input() section: string;           // 'statement', 'budget', 'details', etc.
@Input() entityId: number | null;   // Current entity ID (opportunity, partner, etc.)
@Input() currentRoute: string;      // Current Angular route

// Configuration
@Input() enableDocumentUpload: boolean = true;
@Input() enableAutofill: boolean = true;
@Input() quickActions: QuickAction[] = [];  // Custom quick action buttons
@Input() maxDocuments: number = 5;

// Styling
@Input() popupWidth: string = '600px';
@Input() popupHeight: string = '700px';

// Callbacks
@Output() onAutofill = new EventEmitter<FieldValueMap>();
@Output() onResponseReceived = new EventEmitter<string>();
@Output() onError = new EventEmitter<Error>();
```

**Quick Actions:**
```typescript
interface QuickAction {
  label: string;           // "Explain this section"
  icon: string;            // "pi pi-info-circle"
  prompt: string;          // Predefined prompt to send
  intent: 'question' | 'autofill' | 'explain';
}
```

**Usage Example:**
```html
<!-- In any component header -->
<div class="section-header">
  <h2>{{ 'title.opportunityStatement' | translate }}</h2>
  <app-ai-section-assistance
    feature="opportunities"
    section="statement"
    [entityId]="opportunityId()"
    [currentRoute]="router.url"
    [quickActions]="statementQuickActions"
    (onAutofill)="handleAutofill($event)">
  </app-ai-section-assistance>
</div>
```

```typescript
// Component TypeScript
statementQuickActions: QuickAction[] = [
  {
    label: 'Explain this section',
    icon: 'pi pi-info-circle',
    prompt: 'Explain what I should include in the Opportunity Statement section',
    intent: 'question'
  },
  {
    label: 'Generate from documents',
    icon: 'pi pi-file-plus',
    prompt: 'Generate an opportunity statement based on my uploaded documents',
    intent: 'autofill'
  },
  {
    label: 'Improve statement',
    icon: 'pi pi-sparkles',
    prompt: 'Review and improve the current opportunity statement',
    intent: 'autofill'
  }
];
```

### 5.2 Backend: API Endpoints

#### Endpoint 1: Section Assistance (Main)

```csharp
[HttpPost("ai/section-assistance")]
[PermissionAuthorize(PermissionNames.CanUseAI)]
public async Task<ActionResult<SectionAssistanceResponse>> GetSectionAssistance(
    [FromBody] SectionAssistanceRequest request)
{
    var response = await _aiSectionAssistanceManager.ProcessSectionAssistanceAsync(
        request, 
        _currentUserId
    );
    return Ok(response);
}
```

**Request Model:**
```csharp
public class SectionAssistanceRequest
{
    [Required]
    public string Prompt { get; set; }
    
    [Required]
    public string Feature { get; set; }  // opportunities, partnerships, etc.
    
    [Required]
    public string Section { get; set; }  // statement, budget, etc.
    
    public string CurrentRoute { get; set; }
    
    public int? EntityId { get; set; }
    
    public List<int> DocumentIds { get; set; } = new();
    
    [Required]
    public string Intent { get; set; }  // question, autofill, explain
    
    public Dictionary<string, object>? AdditionalContext { get; set; }
}
```

**Response Model:**
```csharp
public class SectionAssistanceResponse
{
    public string Response { get; set; }  // Markdown or plain text
    
    public string Intent { get; set; }
    
    // For autofill intent
    public Dictionary<string, object>? FieldValues { get; set; }
    
    // Guidance that was used
    public List<GuidanceReference> GuidanceUsed { get; set; } = new();
    
    // Metadata
    public string Model { get; set; }
    public int TokensUsed { get; set; }
    public DateTime Timestamp { get; set; }
}

public class GuidanceReference
{
    public int GuidanceId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
}
```

#### Endpoint 2: Get Section Guidance (Fetch Only)

```csharp
[HttpGet("ai/section-guidance")]
[PermissionAuthorize(PermissionNames.CanViewGuidance)]
public async Task<ActionResult<List<GuidanceModel>>> GetSectionGuidance(
    [FromQuery] string feature,
    [FromQuery] string section,
    [FromQuery] string? route = null)
{
    var guidance = await _applicationGuidanceManager.GetGuidanceForSectionAsync(
        feature, 
        section, 
        route
    );
    return Ok(guidance);
}
```

#### Endpoint 3: Autofill Preview (Special Case for Opportunity Statement)

```csharp
[HttpPost("ai/section-assistance/autofill-preview")]
[PermissionAuthorize(PermissionNames.CanUseAI)]
public async Task<ActionResult<AutofillPreviewResponse>> GetAutofillPreview(
    [FromBody] AutofillPreviewRequest request)
{
    // Special handling for sections like Opportunity Statement
    // that need to call specific endpoints
    var response = await _aiSectionAssistanceManager.GetAutofillPreviewAsync(
        request, 
        _currentUserId
    );
    return Ok(response);
}
```

---

## 6. Data Flow Diagrams

### 6.1 Question Intent Flow

```
User: "What should I include in the budget section?"

Frontend:
{
  "prompt": "What should I include in the budget section?",
  "feature": "opportunities",
  "section": "budget",
  "entityId": 123,
  "intent": "question"
}
    ↓
Backend:
1. Fetch guidance for opportunities/budget
   → Returns 3 guidance documents
   
2. Load opportunity entity (ID 123)
   → Current budget data
   
3. Build Gemini prompt:
   System: "You are helping with the budget section. Here's guidance:
            [Guidance 1 markdown]
            [Guidance 2 markdown]
            Current budget: [JSON data]"
   User: "What should I include in the budget section?"
   
4. Call Gemini API
   → Response: "For the budget section, you should include..."
   
5. Return response
    ↓
Frontend:
Display response in popup with markdown formatting
```

### 6.2 Autofill Intent Flow

```
User: "Fill out the budget based on my uploaded document"
+ uploads "Project-Budget-2025.pdf"

Frontend:
1. Upload document to GCS
   → Returns storagePath: "gs://bucket/opportunities/123/doc.pdf"
   
2. Send request:
{
  "prompt": "Fill out the budget based on my uploaded document",
  "feature": "opportunities",
  "section": "budget",
  "entityId": 123,
  "documentIds": [456],
  "intent": "autofill"
}
    ↓
Backend:
1. Fetch guidance for opportunities/budget
   
2. Load opportunity budget fields schema
   → Fields: totalBudget, programmeSupport, staffCosts, etc.
   
3. Get document gs:// URI
   
4. Build Gemini prompt:
   System: "Extract budget information and return JSON:
            { 'totalBudget': <number>, 'programmeSupport': <number>, ... }
            Available fields: [field list]
            Current values: [current JSON]"
   User: "Fill out the budget based on my uploaded document"
   Documents: [gs:// URI with mimeType]
   
5. Call Gemini API with document
   → Response: { "totalBudget": 500000, "programmeSupport": 25000, ... }
   
6. Validate field names against schema
   
7. Return field-value pairs
    ↓
Frontend:
1. Show preview of suggested values
2. User clicks "Accept"
3. Populate form fields with animation
```

### 6.3 Special Case: Opportunity Statement

```
User: On Opportunity Statement section, clicks AI icon, 
      enters: "Improve this statement based on new partner data"

Frontend:
{
  "prompt": "Improve this statement based on new partner data",
  "feature": "opportunities",
  "section": "statement",
  "entityId": 123,
  "intent": "autofill"
}
    ↓
Backend:
1. Detect section = "statement" + intent = "autofill"
   → Route to special Opportunity Statement handler
   
2. Call EXISTING opportunity statement endpoint:
   POST /opportunity/{id}/generate-statement
   + append user prompt as additional context
   
3. Use existing opportunity_generate_statement prompt
   + add: "User Additional Instructions: {user prompt}"
   
4. Return generated statement markdown
    ↓
Frontend:
Display in statement preview area with Accept/Reject
```

---

## 7. Implementation Phases

### Phase 1: Foundation (Sprint 1-2)

**Database:**
- [ ] Create `ApplicationGuidance` table migration
- [ ] Create C# entity class
- [ ] Seed initial guidance (10-15 key sections)

**Backend:**
- [ ] Create `ApplicationGuidanceManager`
- [ ] Create `AiSectionAssistanceManager`
- [ ] Implement guidance retrieval (direct DB queries)
- [ ] Create API controller with 3 endpoints
- [ ] Create new AI prompt type: `section_assistance`

**Frontend:**
- [ ] Create `AiSectionAssistanceComponent`
- [ ] Create `AiSectionAssistanceService`
- [ ] Integrate with document upload
- [ ] Add to 3 pilot sections (Opportunity Statement, Budget, Details)

**Testing:**
- [ ] Unit tests for guidance retrieval
- [ ] Integration tests for API endpoints
- [ ] E2E tests for popup interaction

### Phase 2: Enhanced Retrieval (Sprint 3-4)

**Backend:**
- [ ] Generate embeddings for guidance content
- [ ] Extend `EntityEmbeddings` for guidance
- [ ] Implement hybrid retrieval (direct + vector)
- [ ] Integrate with Python AI service vector store

**Frontend:**
- [ ] Expand to 10+ sections across features
- [ ] Add autofill capability
- [ ] Implement field validation

### Phase 3: Optimization (Sprint 5-6)

**Backend:**
- [ ] Implement response caching
- [ ] Add guidance versioning
- [ ] Create admin UI for guidance management

**Frontend:**
- [ ] Add telemetry (track usage, success rates)
- [ ] Optimize popup UX based on feedback
- [ ] Add keyboard shortcuts

---

## 8. Security and Performance Considerations

### 8.1 Security

1. **Permission Checks:**
   - `CanUseAI` permission required for all AI endpoints
   - Entity-level permissions: verify user can access `entityId`
   - Document access: verify user can view uploaded documents

2. **Input Validation:**
   - Sanitize all user prompts
   - Validate feature/section values against whitelist
   - Limit document count and size
   - Rate limiting on AI endpoints (per user, per hour)

3. **Data Privacy:**
   - Don't include sensitive data in guidance markdown
   - Audit log all AI requests
   - Comply with data retention policies

### 8.2 Performance

1. **Caching:**
   - Cache guidance retrieval results (15 minutes)
   - Cache entity data for common queries
   - Use existing `AiPromptCacheService` for responses

2. **Database Optimization:**
   - Proper indexes on ApplicationGuidance table
   - Limit guidance content size (max 10KB per document)
   - Use pagination for large result sets

3. **API Optimization:**
   - Stream Gemini responses for long-running queries
   - Implement request queueing for high load
   - Use connection pooling for database

---

## 9. Next Steps

### Immediate Actions (This Week)

1. **Review and Approve Architecture:**
   - [ ] Review this document with team
   - [ ] Approve database schema
   - [ ] Approve API endpoints
   - [ ] Decide on embedding strategy (Phase 1 vs Phase 2)

2. **Create Implementation Plan:**
   - [ ] Break down into detailed tasks
   - [ ] Assign story points
   - [ ] Identify dependencies
   - [ ] Create tickets in project management tool

3. **Prepare Development Environment:**
   - [ ] Set up database migrations
   - [ ] Create seed data for guidance
   - [ ] Update API documentation

### Next Week

1. **Start Phase 1 Development:**
   - Backend: Database + Manager layer
   - Frontend: Shared component shell
   - Integration: Wire up basic flow

2. **Create Sample Guidance Content:**
   - Write guidance for 3 pilot sections
   - Test markdown formatting
   - Validate guidance effectiveness

---

## 10. Appendices

### A. Sample Guidance Records

#### Example 1: Opportunity Statement Guidance

```json
{
  "title": "How to Write an Opportunity Statement",
  "slug": "opportunity-statement-guide",
  "guidanceType": "section",
  "feature": "opportunities",
  "section": "statement",
  "componentPath": "features/partnerships/opportunities/components/opportunity/view/sections/statement",
  "route": "/opportunities/:id/edit",
  "contentMarkdown": "# Opportunity Statement Guide\n\n## Purpose\n\nThe Opportunity Statement is a comprehensive document that outlines the strategic rationale, context, and value proposition of a proposed partnership or project...\n\n## Key Components\n\n### 1. Summary\n- Keep to 50 words maximum\n- Highlight potential impact\n- Align with UN/UNOPS goals\n\n### 2. Context and Challenges\n...",
  "summary": "Guidelines for creating comprehensive opportunity statements following UNOPS template format",
  "keywords": ["opportunity statement", "proposal", "strategic rationale", "value proposition"],
  "category": "how-to",
  "priority": 90,
  "isActive": true,
  "version": "1.0"
}
```

#### Example 2: Budget Section Guidance

```json
{
  "title": "Budget Section Requirements",
  "slug": "budget-section-requirements",
  "guidanceType": "section",
  "feature": "opportunities",
  "section": "budget",
  "contentMarkdown": "# Budget Section Requirements\n\n## Overview\n\nThe budget section captures the financial details of the opportunity...\n\n## Required Fields\n\n- **Total Budget (USD)**: Total estimated budget\n- **Programme Support (%)**: Overhead percentage\n- **Staff Costs**: Personnel expenses\n...",
  "summary": "Requirements and guidelines for completing the budget section",
  "keywords": ["budget", "financial", "costs", "funding"],
  "category": "reference",
  "priority": 80,
  "isActive": true
}
```

### B. AI Prompt Template (section_assistance)

```sql
INSERT INTO public."AiPrompt" (
    "Type",
    "SystemInstructions",
    "UserPrompt",
    "Name",
    "Status",
    "Feature",
    "DataRetrievalMethod",
    "GenerationConfig",
    "ContentConfig",
    "Location",
    "Model",
    "Project",
    "Description",
    "AdminCanChange",
    "UseCache",
    "CacheInvalidationMinutes"
) VALUES (
    'section_assistance',
    'You are an expert UNOPS assistant helping users with the {feature} feature, specifically the {section} section.

**Context:**
- Current Page: {currentRoute}
- Feature: {feature}
- Section: {section}
- Entity ID: {entityId}

**Relevant Guidance:**
{guidanceMarkdown}

**Current Entity Data:**
{entityData}

**User Intent: {intent}**

**Instructions Based on Intent:**

If intent = "question":
- Answer the user''s question based on the guidance provided
- Be specific and reference the guidance
- Provide examples where helpful
- Keep response concise (200-300 words)
- Format response in well-structured markdown

If intent = "autofill":
- Extract information from provided documents or context
- Map to available fields: {availableFields}
- Return ONLY valid JSON with field-value pairs
- Only include fields you have data for
- Format: { "fieldName": "value", ... }

If intent = "explain":
- Explain the purpose and requirements of this section
- Reference the official guidance
- Provide step-by-step instructions
- Include tips and best practices

**Important Rules:**
- Use ONLY actual data from provided context and documents
- Do not invent or hallucinate information
- If information is missing, say so explicitly
- Be helpful, professional, and accurate',
    
    '{userPrompt}',
    'Section Assistance',
    1,
    'general',
    NULL,
    '{ "temperature": 0.4, "top_p": 0.95, "max_output_tokens": 2048 }',
    '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }',
    'europe-west4',
    'gemini-2.0-flash-001',
    'unops-partneropportunity',
    'Provides contextual AI assistance for any section in the application with guidance-backed responses',
    true,
    true,
    30
);
```

### C. Glossary

| Term | Definition |
|------|------------|
| **Section Assistance** | AI-powered help specific to a particular section of the application |
| **Guidance** | Instructional content stored in markdown format that explains how to use features |
| **Intent** | The user's goal: asking a question, auto-filling fields, or requesting an explanation |
| **Feature** | Top-level module (e.g., opportunities, partnerships, interactions) |
| **Section** | Sub-component within a feature (e.g., statement, budget, details) |
| **Autofill** | AI-generated values populated into form fields |
| **Quick Actions** | Predefined prompts shown as buttons for common tasks |
| **Hybrid Retrieval** | Combination of direct database queries and vector similarity search |
| **Embeddings** | Vector representations of guidance content for semantic search |

---

## Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-11-26 | AI Analysis | Initial draft with comprehensive recommendations |

---

**END OF RECOMMENDATIONS DOCUMENT**

