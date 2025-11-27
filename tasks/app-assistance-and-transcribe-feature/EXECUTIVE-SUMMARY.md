# AI Section Assistance Feature - Executive Summary

**Date:** November 26, 2025  
**Document Version:** 1.0  
**Related:** [Full Recommendations](./RECOMMENDATIONS.md)

---

## Quick Overview

This feature adds contextual AI assistance to every section in the application via an AI icon in section headers. Users can:
- Ask questions about any section
- Upload documents for context
- Auto-fill form fields with AI-generated content
- Get intelligent, guidance-backed responses

---

## Key Decisions Summary

### ✅ Recommendation 1: Application Guidance Storage

**Create a new `ApplicationGuidance` table** to store guidance content

**Why:**
- Clean separation from AI prompt configurations
- Better querying and filtering capabilities
- Easier to version and manage
- Clear schema designed for content storage

**Alternative Considered:** Extend AiPrompt table ❌ (rejected - wrong abstraction)

---

### ✅ Recommendation 2: Retrieval Strategy

**Use a hybrid approach in two phases:**

#### Phase 1 (MVP): Direct Database Retrieval
- Query ApplicationGuidance table with context matching
- Fast, deterministic, simple to implement
- **Start with this approach**

#### Phase 2 (Enhancement): Add Vector Embeddings
- Generate embeddings for semantic search
- Leverage existing EntityEmbeddings infrastructure
- Better handling of natural language queries
- **Add after MVP is proven**

**Why Phased:**
- Get to production faster with Phase 1
- Validate feature usefulness before investing in embeddings
- Embeddings add complexity and cost

---

## Architecture at a Glance

```
┌─────────────────────────────────────┐
│  Frontend (Angular)                  │
│  ┌──────────────────────────────┐   │
│  │ Section Header               │   │
│  │   [AI Icon]                  │   │
│  │      ↓                       │   │
│  │ AiSectionAssistanceComponent │   │
│  │  • Prompt input              │   │
│  │  • Document upload           │   │
│  │  • Quick actions             │   │
│  │  • Response display          │   │
│  └──────────────────────────────┘   │
└────────────┬────────────────────────┘
             │ API Call
┌────────────▼────────────────────────┐
│  Backend (.NET)                     │
│  ┌──────────────────────────────┐  │
│  │ AiSectionAssistanceController│  │
│  │      ↓                       │  │
│  │ AiSectionAssistanceManager   │  │
│  │  1. Fetch guidance           │  │
│  │  2. Build Gemini prompt      │  │
│  │  3. Process response         │  │
│  └──────────────────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │ ApplicationGuidance (Table)  │  │
│  │  • Feature + Section mapping │  │
│  │  • Markdown content          │  │
│  │  • Keywords, priority        │  │
│  └──────────────────────────────┘  │
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│  Gemini 2.0 API + GCS               │
└─────────────────────────────────────┘
```

---

## What Gets Built

### New Database Components
1. **ApplicationGuidance Table**
   - Stores guidance markdown
   - Maps to features and sections
   - Includes keywords, priority, versioning

### New Backend Components
1. **ApplicationGuidanceManager** - Manages guidance retrieval
2. **AiSectionAssistanceManager** - Orchestrates AI assistance flow
3. **AiSectionAssistanceController** - API endpoints
4. **New Prompt Type** - `section_assistance` in AiPrompt table

### New Frontend Components
1. **AiSectionAssistanceComponent** - Reusable popup component
2. **AiSectionAssistanceService** - API communication
3. **Integration** - Add AI icon to section headers

---

## API Endpoints

| Endpoint | Purpose |
|----------|---------|
| `POST /api/ai/section-assistance` | Main endpoint for processing user prompts |
| `GET /api/ai/section-guidance` | Fetch guidance for a section (display only) |
| `POST /api/ai/section-assistance/autofill` | Preview autofill suggestions |

---

## User Experience Flow

### Scenario 1: User Asks a Question

```
1. User on "Budget" section clicks AI icon
2. Popup opens, shows quick actions:
   - "Explain this section"
   - "What fields are required?"
   - "Budget best practices"
3. User types: "What's the difference between staff costs and programme support?"
4. System:
   • Fetches budget guidance from database
   • Builds Gemini prompt with guidance context
   • Calls Gemini API
   • Returns markdown response
5. User sees answer with guidance references
```

### Scenario 2: User Uploads Document for Autofill

```
1. User on "Budget" section clicks AI icon
2. User uploads "ProjectBudget2025.xlsx"
3. User selects "Fill out budget from document" quick action
4. System:
   • Uploads file to GCS (gets gs:// URI)
   • Fetches budget field schema
   • Builds Gemini prompt requesting JSON
   • Passes document to Gemini
   • Extracts field values
5. User sees preview of suggested values
6. User clicks "Accept" → fields populate with animation
```

### Scenario 3: Special Case - Opportunity Statement

```
1. User on "Opportunity Statement" section clicks AI icon
2. User enters: "Improve this statement"
3. System detects section="statement" + intent="autofill"
   → Routes to existing opportunity statement generation endpoint
   → Appends user's prompt as additional context
4. Returns improved statement markdown
5. User can accept/reject changes
```

---

## Implementation Phases

### Phase 1: MVP (Sprint 1-2) - **START HERE**
- ✅ Create ApplicationGuidance table
- ✅ Build backend managers and API
- ✅ Create frontend popup component
- ✅ Direct database retrieval only
- ✅ Deploy to 3 pilot sections

**Deliverable:** Working AI assistance in 3 sections

### Phase 2: Enhancement (Sprint 3-4)
- ✅ Generate embeddings for guidance
- ✅ Implement hybrid retrieval
- ✅ Expand to 10+ sections
- ✅ Add autofill capability

**Deliverable:** Enhanced semantic search + autofill

### Phase 3: Optimization (Sprint 5-6)
- ✅ Response caching
- ✅ Guidance versioning
- ✅ Admin UI for guidance management
- ✅ Usage analytics

**Deliverable:** Production-ready, scalable feature

---

## Key Benefits

### For Users
- ✅ Contextual help exactly where needed
- ✅ Faster form completion via autofill
- ✅ Learn application functionality in-context
- ✅ Upload documents for intelligent extraction

### For UNOPS
- ✅ Reduced support tickets
- ✅ Improved data quality (guided inputs)
- ✅ Reusable component across all features
- ✅ Scalable guidance management

### Technical
- ✅ Leverages existing infrastructure (Gemini, GCS, prompts)
- ✅ Clean separation of concerns
- ✅ Extensible architecture
- ✅ Performance-optimized with caching

---

## Critical Success Factors

### Must Have (Phase 1)
1. ✅ Accurate guidance retrieval for context
2. ✅ Clear, helpful AI responses
3. ✅ Intuitive popup UI/UX
4. ✅ Fast response times (<5 seconds)

### Nice to Have (Phase 2+)
1. ✅ Semantic search with embeddings
2. ✅ Autofill validation and preview
3. ✅ Guidance versioning and A/B testing
4. ✅ Usage analytics and feedback

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| **AI responses inaccurate** | High-quality guidance content + prompt engineering + testing |
| **Slow performance** | Caching + async processing + response streaming |
| **User adoption low** | UX testing + prominent placement + useful quick actions |
| **Guidance maintenance burden** | Admin UI + version control + community contributions |

---

## Resource Requirements

### Development Team
- 1 Backend Developer (.NET) - 3 weeks
- 1 Frontend Developer (Angular) - 3 weeks  
- 1 Content Creator (Guidance) - 2 weeks
- 1 QA Engineer - 2 weeks

### Infrastructure
- Database: +1 table (minimal cost)
- Storage: Guidance content (~10KB each, 100 records = 1MB)
- AI API: Estimated 1000 requests/day at $0.0001/request = $100/month
- (Phase 2) Embeddings: +768 floats per guidance = ~10MB for 1000 records

---

## Next Steps

### This Week
1. ✅ Review and approve this architecture
2. ✅ Create detailed implementation plan (next document)
3. ✅ Set up development environment
4. ✅ Write initial guidance content (10-15 sections)

### Next Week
1. ✅ Start Phase 1 backend development
2. ✅ Start Phase 1 frontend development
3. ✅ Set up CI/CD for migrations

### Week 3-4
1. ✅ Integration testing
2. ✅ User acceptance testing (3 pilot sections)
3. ✅ Gather feedback and iterate

---

## Questions to Resolve

Before starting implementation, confirm:

1. ✅ **Approval on database schema** - Is the ApplicationGuidance table structure acceptable?
2. ✅ **Phase 1 vs Phase 2** - Start with direct retrieval only, or build embeddings from day 1?
3. ✅ **Pilot sections** - Which 3 sections should we start with?
   - Suggested: Opportunity Statement, Budget, Details
4. ✅ **Permissions** - Should there be a new permission or reuse existing CanUseAI?
5. ✅ **Content ownership** - Who will write/maintain the guidance content?

---

## Conclusion

This feature provides a **scalable, reusable AI assistance framework** that can be deployed across the entire application. The phased approach minimizes risk while delivering immediate value.

**Recommendation:** Approve Phase 1 architecture and proceed with implementation planning.

---

**Related Documents:**
- [Full Recommendations](./RECOMMENDATIONS.md) - Detailed technical specifications
- [Implementation Plan](./IMPLEMENTATION-PLAN.md) - Step-by-step development guide (to be created)

---

**Document Version:** 1.0  
**Last Updated:** November 26, 2025  
**Status:** Ready for Review

