# AI Section Assistance Feature - Implementation Plan

**Date:** November 26, 2025  
**Version:** 1.0  
**Sprint Duration:** 2 weeks per sprint  
**Team Size:** 2 developers (1 Backend .NET, 1 Frontend Angular)

---

## Overview

This document provides a **step-by-step implementation plan** for the AI Section Assistance feature, broken down into actionable tasks with estimates and dependencies.

**Total Estimated Duration:** 6 weeks (3 sprints)  
**Priority:** High  
**Risk Level:** Medium

---

## Sprint 1: Foundation & Core Infrastructure (Weeks 1-2)

**Goal:** Build database, backend foundation, basic frontend component

### Week 1: Database & Backend Setup

#### Task 1.1: Database Schema & Migration ⏱️ 4 hours
**Assignee:** Backend Developer  
**Priority:** P0 (Blocker)

**Subtasks:**
- [ ] Create migration file `AddApplicationGuidanceTable.cs`
- [ ] Define ApplicationGuidance entity class
- [ ] Add DbSet to UNOPSAppDbContext
- [ ] Create indexes for performance
- [ ] Test migration up/down locally
- [ ] Write seed script with 5 sample guidance records

**Acceptance Criteria:**
- ✅ Migration runs successfully on dev database
- ✅ Table structure matches specification
- ✅ Indexes improve query performance
- ✅ Seed data loads correctly

**Files to Create:**
```
UNOPS.PAO.UNOPSDataAccess/
  Migrations/
    20251126_AddApplicationGuidanceTable.cs
  Seed/Scripts/
    ApplicationGuidance.sql

UNOPS.PAO.Domain/
  Entities/
    ApplicationGuidance.cs
```

---

#### Task 1.2: Application Guidance Manager ⏱️ 6 hours
**Assignee:** Backend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 1.1

**Subtasks:**
- [ ] Create `IApplicationGuidanceManager` interface
- [ ] Implement `ApplicationGuidanceManager` class
- [ ] Add repository for ApplicationGuidance
- [ ] Implement `GetGuidanceForSectionAsync()` method
- [ ] Implement `GetGuidanceByIdAsync()` method
- [ ] Implement `SearchGuidanceAsync()` method
- [ ] Add to ManagerWrapper
- [ ] Write unit tests

**Key Methods:**
```csharp
Task<List<GuidanceModel>> GetGuidanceForSectionAsync(
    string feature, 
    string section, 
    string? route = null, 
    int maxResults = 5
);

Task<GuidanceModel> GetGuidanceByIdAsync(int id);

Task<List<GuidanceModel>> SearchGuidanceAsync(
    string query, 
    string? feature = null, 
    int maxResults = 10
);
```

**Acceptance Criteria:**
- ✅ Manager registered in DI container
- ✅ Methods return correctly filtered guidance
- ✅ Priority sorting works correctly
- ✅ Unit test coverage > 80%

**Files to Create:**
```
UNOPS.PAO.UNOPSBusiness/
  Interfaces/
    IApplicationGuidanceManager.cs
  Managers/
    ApplicationGuidanceManager.cs

UNOPS.PAO.Models/
  AI/
    GuidanceModel.cs
    GuidanceFilterRequest.cs
```

---

#### Task 1.3: AI Section Assistance Manager (Part 1) ⏱️ 8 hours
**Assignee:** Backend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 1.2

**Subtasks:**
- [ ] Create `IAiSectionAssistanceManager` interface
- [ ] Implement `AiSectionAssistanceManager` class
- [ ] Implement guidance retrieval logic
- [ ] Implement prompt building logic
- [ ] Integrate with UNOPSGeminiManager
- [ ] Add context data retrieval
- [ ] Write unit tests

**Key Method (Phase 1):**
```csharp
Task<SectionAssistanceResponse> ProcessSectionAssistanceAsync(
    SectionAssistanceRequest request,
    int userId
);
```

**Acceptance Criteria:**
- ✅ Manager retrieves relevant guidance
- ✅ Builds proper Gemini prompt with context
- ✅ Handles missing guidance gracefully
- ✅ Returns formatted response

**Files to Create:**
```
UNOPS.PAO.UNOPSBusiness/
  Interfaces/
    IAiSectionAssistanceManager.cs
  Managers/
    AiSectionAssistanceManager.cs

UNOPS.PAO.Models/
  AI/
    SectionAssistanceRequest.cs
    SectionAssistanceResponse.cs
    GuidanceReference.cs
```

---

### Week 2: API & Frontend Core

#### Task 1.4: API Controller & Endpoints ⏱️ 6 hours
**Assignee:** Backend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 1.3

**Subtasks:**
- [ ] Create `AiSectionAssistanceController`
- [ ] Implement POST `/api/ai/section-assistance` endpoint
- [ ] Implement GET `/api/ai/section-guidance` endpoint
- [ ] Add permission authorization
- [ ] Add input validation
- [ ] Add error handling
- [ ] Update API dictionary
- [ ] Test endpoints with Postman/Swagger

**Acceptance Criteria:**
- ✅ Endpoints return 200 OK for valid requests
- ✅ Permission checks work correctly
- ✅ Validation errors return 400 Bad Request
- ✅ API documentation updated

**Files to Create:**
```
UNOPS.PAO.UNOPSPresentation/
  Controllers/
    AiSectionAssistanceController.cs

UNOPS.PAO.Models/
  Constants/
    APIDictionary.cs (update)
```

---

#### Task 1.5: AI Prompt Configuration ⏱️ 3 hours
**Assignee:** Backend Developer  
**Priority:** P1 (Important)  
**Dependencies:** Task 1.4

**Subtasks:**
- [ ] Create `section_assistance` prompt in AiPrompt table
- [ ] Define system instructions template
- [ ] Define user prompt template
- [ ] Configure generation parameters
- [ ] Test prompt with sample data
- [ ] Create migration/seed script

**Acceptance Criteria:**
- ✅ Prompt exists in database
- ✅ Placeholder substitution works correctly
- ✅ Generates quality responses

**Files to Create:**
```
UNOPS.PAO.UNOPSDataAccess/
  Migrations/
    20251127_AddSectionAssistancePrompt.cs
  or
  Seed/Scripts/
    AiPrompts.sql (update)
```

---

#### Task 1.6: Frontend Service Layer ⏱️ 4 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)

**Subtasks:**
- [ ] Create `AiSectionAssistanceService`
- [ ] Implement API methods
- [ ] Add error handling
- [ ] Add request/response types
- [ ] Write unit tests

**Key Methods:**
```typescript
getSectionAssistance(request: SectionAssistanceRequest): Observable<SectionAssistanceResponse>
getSectionGuidance(feature: string, section: string): Observable<GuidanceModel[]>
```

**Acceptance Criteria:**
- ✅ Service methods call correct endpoints
- ✅ Error handling returns user-friendly messages
- ✅ TypeScript types match backend models

**Files to Create:**
```
UNOPS.PAO.ClientApp/src/app/
  features/ai/services/
    ai-section-assistance.service.ts
  features/ai/models/
    section-assistance.interface.ts
```

---

#### Task 1.7: Basic Popup Component (Shell) ⏱️ 8 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 1.6

**Subtasks:**
- [ ] Create `AiSectionAssistanceComponent`
- [ ] Create component template (HTML)
- [ ] Add basic styling (SCSS)
- [ ] Implement input signals
- [ ] Implement output signals
- [ ] Add p-dialog integration
- [ ] Add loading states
- [ ] Test component in isolation

**Acceptance Criteria:**
- ✅ Component opens/closes correctly
- ✅ Accepts configuration inputs
- ✅ Shows loading spinner
- ✅ Displays basic UI structure

**Files to Create:**
```
UNOPS.PAO.ClientApp/src/app/
  features/ai/components/ai-section-assistance/
    ai-section-assistance.component.ts
    ai-section-assistance.component.html
    ai-section-assistance.component.scss
    ai-section-assistance.component.spec.ts
```

---

### Sprint 1 Deliverables

**By End of Week 2:**
- ✅ Database table with seed data
- ✅ Backend managers and API endpoints
- ✅ Frontend service and basic component shell
- ✅ Unit tests for backend logic
- ✅ Can make successful API call from frontend to backend

**Not Yet Implemented:**
- ❌ Document upload integration
- ❌ Quick action buttons
- ❌ Autofill capability
- ❌ Full UI/UX polish

---

## Sprint 2: Complete UI & Integration (Weeks 3-4)

**Goal:** Finish frontend component, add document upload, integrate into 3 sections

### Week 3: Complete Frontend Component

#### Task 2.1: Document Upload Integration ⏱️ 6 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)

**Subtasks:**
- [ ] Integrate existing document upload component
- [ ] Add file selection button
- [ ] Add file preview
- [ ] Upload to GCS on selection
- [ ] Store document IDs for request
- [ ] Add remove file capability
- [ ] Handle upload errors

**Acceptance Criteria:**
- ✅ User can select files
- ✅ Files upload to GCS
- ✅ File names displayed in UI
- ✅ Can remove uploaded files
- ✅ Document IDs sent with request

---

#### Task 2.2: Quick Action Buttons ⏱️ 4 hours
**Assignee:** Frontend Developer  
**Priority:** P1 (Important)  
**Dependencies:** Task 2.1

**Subtasks:**
- [ ] Create quick action button layout
- [ ] Implement input for custom actions
- [ ] Add click handlers
- [ ] Auto-populate prompt on click
- [ ] Add icons and styling
- [ ] Test with different action configurations

**Acceptance Criteria:**
- ✅ Buttons render from input array
- ✅ Clicking button fills prompt input
- ✅ Can send request immediately
- ✅ Mobile-responsive layout

---

#### Task 2.3: Response Display & Markdown Rendering ⏱️ 5 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 2.2

**Subtasks:**
- [ ] Add markdown pipe integration
- [ ] Style markdown output
- [ ] Add loading animation
- [ ] Add error display
- [ ] Add "Copy response" button
- [ ] Add "Regenerate" button
- [ ] Test with long responses

**Acceptance Criteria:**
- ✅ Markdown renders correctly
- ✅ Code blocks have syntax highlighting
- ✅ Links are clickable
- ✅ Scrollable for long content
- ✅ Copy button works

---

#### Task 2.4: Component Polish & Accessibility ⏱️ 5 hours
**Assignee:** Frontend Developer  
**Priority:** P1 (Important)  
**Dependencies:** Task 2.3

**Subtasks:**
- [ ] Add keyboard shortcuts (Esc to close, Enter to send)
- [ ] Add ARIA labels
- [ ] Add focus management
- [ ] Add loading skeleton
- [ ] Add animations (fade in/out)
- [ ] Test with screen reader
- [ ] Test with keyboard-only navigation
- [ ] Mobile responsiveness

**Acceptance Criteria:**
- ✅ WCAG 2.1 AA compliant
- ✅ Keyboard accessible
- ✅ Screen reader friendly
- ✅ Smooth animations
- ✅ Works on mobile devices

---

### Week 4: Integration & Testing

#### Task 2.5: Integrate into Opportunity Statement Section ⏱️ 3 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 2.4

**Subtasks:**
- [ ] Add AI icon to statement section header
- [ ] Configure component inputs for statement section
- [ ] Define quick actions for statement
- [ ] Test with actual opportunity data
- [ ] Handle autofill for statement content

**Quick Actions for Statement:**
```typescript
[
  { label: 'Explain this section', prompt: '...', intent: 'explain' },
  { label: 'Generate statement', prompt: '...', intent: 'autofill' },
  { label: 'Improve statement', prompt: '...', intent: 'autofill' }
]
```

**Acceptance Criteria:**
- ✅ Icon appears in header
- ✅ Component opens with correct context
- ✅ Quick actions work correctly
- ✅ Generates statement successfully

**Files to Modify:**
```
UNOPS.PAO.ClientApp/src/app/
  features/partnerships/opportunities/components/opportunity/view/sections/statement/
    opportunity-statement-section.component.html (add AI icon)
    opportunity-statement-section.component.ts (add quick actions)
```

---

#### Task 2.6: Integrate into Budget Section ⏱️ 3 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 2.5

**Subtasks:**
- [ ] Add AI icon to budget section header
- [ ] Configure component inputs
- [ ] Define quick actions for budget
- [ ] Test with budget data
- [ ] Implement autofill preview for budget fields

**Quick Actions for Budget:**
```typescript
[
  { label: 'Explain budget fields', prompt: '...', intent: 'explain' },
  { label: 'Extract from document', prompt: '...', intent: 'autofill' },
  { label: 'Budget best practices', prompt: '...', intent: 'question' }
]
```

**Acceptance Criteria:**
- ✅ AI assistance works for budget section
- ✅ Can extract budget values from uploaded documents
- ✅ Field validation works

---

#### Task 2.7: Integrate into Details Section ⏱️ 3 hours
**Assignee:** Frontend Developer  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 2.6

**Subtasks:**
- [ ] Add AI icon to details section header
- [ ] Configure component inputs
- [ ] Define quick actions for details
- [ ] Test with opportunity details

**Acceptance Criteria:**
- ✅ AI assistance works for details section
- ✅ Can answer questions about required fields

---

#### Task 2.8: Backend - Autofill Response Processing ⏱️ 6 hours
**Assignee:** Backend Developer  
**Priority:** P0 (Blocker)

**Subtasks:**
- [ ] Extend AiSectionAssistanceManager for autofill intent
- [ ] Fetch entity field schema dynamically
- [ ] Build JSON-specific prompts for autofill
- [ ] Parse and validate Gemini JSON response
- [ ] Map field names to entity properties
- [ ] Return structured field-value pairs
- [ ] Handle validation errors
- [ ] Write unit tests

**Acceptance Criteria:**
- ✅ Returns valid JSON field mappings
- ✅ Validates against entity schema
- ✅ Handles missing or invalid fields gracefully
- ✅ Works with different entity types

---

#### Task 2.9: Content Creation - Write Guidance ⏱️ 8 hours
**Assignee:** Content Creator / Tech Writer  
**Priority:** P1 (Important)

**Subtasks:**
- [ ] Write guidance for Opportunity Statement section
- [ ] Write guidance for Budget section  
- [ ] Write guidance for Details section
- [ ] Write general guidance for opportunities feature
- [ ] Format in markdown
- [ ] Add keywords for search
- [ ] Review with stakeholders
- [ ] Load into database via seed script

**Acceptance Criteria:**
- ✅ 3 section-specific guidance documents
- ✅ 1 feature-level guidance document
- ✅ Clear, concise, actionable content
- ✅ Properly formatted markdown
- ✅ Loaded into database

---

#### Task 2.10: Integration Testing ⏱️ 6 hours
**Assignee:** Both Developers  
**Priority:** P0 (Blocker)  
**Dependencies:** All previous tasks

**Subtasks:**
- [ ] Test end-to-end flow for each section
- [ ] Test with various user prompts
- [ ] Test document upload scenarios
- [ ] Test error conditions
- [ ] Test performance (response times)
- [ ] Fix identified bugs
- [ ] Document known issues

**Test Scenarios:**
1. User asks question → receives guidance-backed answer
2. User uploads document → autofills budget fields
3. User generates opportunity statement
4. Error handling: no guidance found
5. Error handling: Gemini API timeout
6. Performance: response in < 5 seconds

**Acceptance Criteria:**
- ✅ All test scenarios pass
- ✅ No critical bugs
- ✅ Performance meets SLA
- ✅ User experience is smooth

---

### Sprint 2 Deliverables

**By End of Week 4:**
- ✅ Fully functional AI assistance component
- ✅ Document upload capability
- ✅ Integrated into 3 sections
- ✅ Quick action buttons working
- ✅ Autofill capability (basic)
- ✅ Guidance content written and loaded
- ✅ Integration tests passing

**Ready for:** User Acceptance Testing (UAT)

---

## Sprint 3: Polish, Optimization & Expansion (Weeks 5-6)

**Goal:** UAT feedback, optimize, add to more sections

### Week 5: UAT & Refinement

#### Task 3.1: User Acceptance Testing ⏱️ 8 hours
**Assignee:** QA + End Users  
**Priority:** P0 (Blocker)

**Subtasks:**
- [ ] Conduct UAT sessions with 5-10 users
- [ ] Collect feedback on UI/UX
- [ ] Measure response quality
- [ ] Identify usability issues
- [ ] Document feature requests
- [ ] Prioritize bugs and improvements

**Acceptance Criteria:**
- ✅ UAT completed with target users
- ✅ Feedback documented
- ✅ Critical bugs identified
- ✅ Improvement backlog created

---

#### Task 3.2: Bug Fixes & Improvements ⏱️ 12 hours
**Assignee:** Both Developers  
**Priority:** P0 (Blocker)  
**Dependencies:** Task 3.1

**Subtasks:**
- [ ] Fix critical bugs from UAT
- [ ] Improve response quality (prompt tuning)
- [ ] Optimize UI based on feedback
- [ ] Improve error messages
- [ ] Add tooltips and help text
- [ ] Retest affected areas

**Acceptance Criteria:**
- ✅ All critical bugs fixed
- ✅ User feedback addressed
- ✅ No regression issues

---

### Week 6: Optimization & Expansion

#### Task 3.3: Response Caching Implementation ⏱️ 6 hours
**Assignee:** Backend Developer  
**Priority:** P2 (Nice to have)

**Subtasks:**
- [ ] Leverage existing AiPromptCacheService
- [ ] Cache guidance retrieval results
- [ ] Cache common user queries
- [ ] Set appropriate TTLs
- [ ] Add cache invalidation logic
- [ ] Test cache hit/miss scenarios
- [ ] Measure performance improvement

**Acceptance Criteria:**
- ✅ Cache hit rate > 30%
- ✅ Response time improvement > 40%
- ✅ Cache invalidation works correctly

---

#### Task 3.4: Add to 3 More Sections ⏱️ 6 hours
**Assignee:** Frontend Developer  
**Priority:** P1 (Important)

**Target Sections:**
- Partners section
- Contacts section
- Interactions section

**Subtasks:**
- [ ] Add AI icon to each section
- [ ] Configure context inputs
- [ ] Define section-specific quick actions
- [ ] Write additional guidance content
- [ ] Test each integration

**Acceptance Criteria:**
- ✅ AI assistance works in 6 total sections
- ✅ Each section has relevant quick actions
- ✅ Guidance content appropriate

---

#### Task 3.5: Telemetry & Analytics ⏱️ 4 hours
**Assignee:** Backend Developer  
**Priority:** P2 (Nice to have)

**Subtasks:**
- [ ] Log AI assistance requests
- [ ] Track feature/section usage
- [ ] Track response quality (user feedback)
- [ ] Track autofill acceptance rate
- [ ] Create dashboard queries
- [ ] Document metrics

**Metrics to Track:**
```
- Requests per day/week
- Response time (avg, p50, p95, p99)
- Error rate
- Feature/section usage distribution
- Autofill acceptance rate
- User satisfaction (thumbs up/down)
```

**Acceptance Criteria:**
- ✅ Telemetry data captured
- ✅ Queries created for analysis
- ✅ Dashboard accessible

---

#### Task 3.6: Documentation ⏱️ 4 hours
**Assignee:** Both Developers  
**Priority:** P1 (Important)

**Subtasks:**
- [ ] Update API documentation
- [ ] Write developer guide (how to add to new sections)
- [ ] Write guidance author guide
- [ ] Update system architecture docs
- [ ] Create troubleshooting guide
- [ ] Update user guide

**Acceptance Criteria:**
- ✅ Complete technical documentation
- ✅ User-facing documentation
- ✅ Onboarding guide for new sections

---

### Sprint 3 Deliverables

**By End of Week 6:**
- ✅ UAT feedback incorporated
- ✅ All critical bugs fixed
- ✅ Response caching implemented
- ✅ Expanded to 6 total sections
- ✅ Telemetry and analytics in place
- ✅ Complete documentation

**Ready for:** Production deployment

---

## Post-MVP: Phase 2 (Future Sprints)

### Sprint 4-5: Embeddings & Semantic Search

**Goals:**
- Generate embeddings for all guidance content
- Implement hybrid retrieval (direct + vector)
- Integrate with Python AI service vector store
- Improve semantic understanding

**Estimated Effort:** 3-4 weeks

---

## Resource Allocation Summary

### Backend Developer
- Sprint 1: 27 hours (database, managers, API)
- Sprint 2: 6 hours (autofill logic)
- Sprint 3: 16 hours (UAT bugs, caching, telemetry)
- **Total:** 49 hours (~6 working days)

### Frontend Developer
- Sprint 1: 12 hours (service, component shell)
- Sprint 2: 29 hours (complete component, integration)
- Sprint 3: 18 hours (UAT bugs, expansion)
- **Total:** 59 hours (~7.5 working days)

### Content Creator
- Sprint 2: 8 hours (guidance writing)
- Sprint 3: 4 hours (additional guidance)
- **Total:** 12 hours (~1.5 days)

### QA / End Users
- Sprint 3: 8 hours (UAT)
- **Total:** 8 hours (~1 day)

---

## Risk Management

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Gemini API quality issues** | Medium | High | Extensive prompt testing; fallback responses |
| **Performance degradation** | Medium | Medium | Caching; async processing; monitoring |
| **Low user adoption** | Medium | High | UX testing; prominent placement; valuable quick actions |
| **Guidance content gaps** | High | Medium | Iterative content creation; community contributions |
| **Integration complexity** | Low | Medium | Modular design; clear interfaces; good testing |

---

## Success Metrics (KPIs)

### Phase 1 (MVP) Success Criteria

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Response Time** | < 5 seconds (95th percentile) | Backend telemetry |
| **Error Rate** | < 2% | API logs |
| **User Adoption** | > 20% of active users in 3 sections | Usage analytics |
| **Autofill Acceptance** | > 40% | User actions |
| **User Satisfaction** | > 4/5 stars | In-app feedback |

### Phase 2 (Enhanced) Success Criteria

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Cache Hit Rate** | > 30% | Cache telemetry |
| **Semantic Search Accuracy** | > 80% relevant results | User feedback |
| **Expansion** | AI assistance in 15+ sections | Feature deployment |
| **Support Ticket Reduction** | 10% fewer tickets re: section help | Support metrics |

---

## Go-Live Checklist

Before production deployment:

### Technical Readiness
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] UAT sign-off received
- [ ] Performance benchmarks met
- [ ] Security review completed
- [ ] Database migrations tested on staging
- [ ] Rollback plan documented

### Content Readiness
- [ ] Guidance content reviewed and approved
- [ ] Translation keys added (all languages)
- [ ] User documentation completed
- [ ] Training materials prepared

### Operational Readiness
- [ ] Monitoring and alerts configured
- [ ] Runbook created
- [ ] Support team trained
- [ ] Incident response plan documented

### Business Readiness
- [ ] Stakeholder sign-off
- [ ] Communication plan executed
- [ ] User announcements prepared

---

## Rollout Strategy

### Phase 1: Soft Launch (Week 7)
- Deploy to **staging environment**
- Enable for **internal users only**
- Monitor for issues
- Gather feedback

### Phase 2: Limited Production (Week 8)
- Deploy to **production**
- Enable for **10% of users** (feature flag)
- Monitor metrics closely
- Fix any issues

### Phase 3: Full Rollout (Week 9)
- Enable for **50% of users**
- Continue monitoring
- Expand to **100% of users** if metrics look good

---

## Next Actions

### Immediate (This Week)
1. ✅ Review and approve this implementation plan
2. ✅ Create tickets in project management tool
3. ✅ Assign tasks to developers
4. ✅ Set up dev environment branches
5. ✅ Schedule kick-off meeting

### Week 1 Start
1. ✅ Backend: Begin Task 1.1 (Database schema)
2. ✅ Frontend: Review existing components for reuse
3. ✅ Content: Start gathering information for guidance writing

---

## Appendix: Task Dependencies Graph

```
Task 1.1 (Database)
    ↓
Task 1.2 (Guidance Manager)
    ↓
Task 1.3 (Assistance Manager)
    ↓
Task 1.4 (API Controller) ──→ Task 1.5 (AI Prompt)
    ↓
Task 1.6 (Frontend Service)
    ↓
Task 1.7 (Component Shell)
    ↓
Task 2.1 (Document Upload)
    ↓
Task 2.2 (Quick Actions)
    ↓
Task 2.3 (Response Display)
    ↓
Task 2.4 (Component Polish)
    ↓
Task 2.5 (Integrate: Statement) ──→ Task 2.8 (Autofill Backend)
    ↓                                    ↓
Task 2.6 (Integrate: Budget) ←──────────┘
    ↓
Task 2.7 (Integrate: Details)
    ↓
Task 2.9 (Write Guidance) ──→ Task 2.10 (Integration Testing)
                                    ↓
                              Task 3.1 (UAT)
                                    ↓
                              Task 3.2 (Bug Fixes)
                                    ↓
                    ┌───────────────┼───────────────┐
                    ↓               ↓               ↓
              Task 3.3        Task 3.4         Task 3.5
              (Caching)       (Expansion)      (Telemetry)
                    └───────────────┬───────────────┘
                                    ↓
                              Task 3.6 (Documentation)
                                    ↓
                              PRODUCTION READY
```

---

**Document Version:** 1.0  
**Last Updated:** November 26, 2025  
**Status:** Ready for Team Review

**Related Documents:**
- [Executive Summary](./EXECUTIVE-SUMMARY.md)
- [Full Recommendations](./RECOMMENDATIONS.md)

