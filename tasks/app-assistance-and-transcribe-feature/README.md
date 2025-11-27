# AI Section Assistance Feature - Documentation Index

**Feature Name:** AI Section Assistance & Contextual Help  
**Project:** UNOPS Opportunity+ System  
**Date Created:** November 26, 2025  
**Status:** 📋 Planning Phase

---

## 📑 Document Overview

This folder contains comprehensive documentation for the AI Section Assistance feature, which adds contextual AI help to every section in the application.

---

## 📚 Documentation Files

### 1. **[Executive Summary](./EXECUTIVE-SUMMARY.md)** ⭐ Start Here
   - **Purpose:** High-level overview and key decisions
   - **Audience:** Product owners, stakeholders, management
   - **Length:** ~10 minutes read
   - **Contains:**
     - Quick overview of the feature
     - Key architectural decisions
     - Benefits and risks
     - Resource requirements
     - Next steps

   **👉 Read this first if you want the big picture**

---

### 2. **[Full Recommendations](./RECOMMENDATIONS.md)** 📖 Technical Deep Dive
   - **Purpose:** Detailed technical specifications and architectural recommendations
   - **Audience:** Architects, senior developers, technical leads
   - **Length:** ~45 minutes read
   - **Contains:**
     - Current system analysis
     - Storage strategy for application guidance
     - Retrieval strategy (embeddings vs direct)
     - Complete architecture design
     - Component specifications
     - Backend API design
     - Data flow diagrams
     - Security and performance considerations

   **👉 Read this for complete technical understanding**

---

### 3. **[Implementation Plan](./IMPLEMENTATION-PLAN.md)** 📝 Task Breakdown
   - **Purpose:** Step-by-step development plan with task estimates
   - **Audience:** Development team, project managers, scrum masters
   - **Length:** ~30 minutes read
   - **Contains:**
     - Sprint-by-sprint breakdown (3 sprints)
     - Detailed task list with estimates
     - Task dependencies
     - Resource allocation
     - Risk management
     - Success metrics
     - Go-live checklist
     - Rollout strategy

   **👉 Read this to start building**

---

## 🎯 Quick Start Guide

### If you're a **Product Owner / Stakeholder:**
1. Read the [Executive Summary](./EXECUTIVE-SUMMARY.md)
2. Review the "Key Decisions Summary" section
3. Check the "Resource Requirements" section
4. Approve or provide feedback

### If you're a **Technical Lead / Architect:**
1. Start with [Executive Summary](./EXECUTIVE-SUMMARY.md)
2. Deep dive into [Full Recommendations](./RECOMMENDATIONS.md)
3. Review architecture diagrams and API specs
4. Validate technical approach

### If you're a **Developer:**
1. Skim [Executive Summary](./EXECUTIVE-SUMMARY.md) for context
2. Focus on [Implementation Plan](./IMPLEMENTATION-PLAN.md)
3. Review your assigned sprint tasks
4. Refer to [Full Recommendations](./RECOMMENDATIONS.md) for technical details as needed

### If you're a **Project Manager:**
1. Read [Executive Summary](./EXECUTIVE-SUMMARY.md)
2. Study [Implementation Plan](./IMPLEMENTATION-PLAN.md) thoroughly
3. Create project tickets from task list
4. Set up sprint planning

---

## 📊 Feature Summary

### What Is This Feature?

An **AI-powered contextual assistance system** embedded in section headers throughout the application. Users can:

- ✅ Ask questions about any section
- ✅ Upload documents for AI analysis
- ✅ Auto-fill form fields with AI-generated content
- ✅ Get guidance-backed, intelligent responses

### Where Does It Appear?

**Everywhere!** Any section in the application can have an AI assistance icon:
- Opportunity Statement section
- Budget section
- Details section
- Partners section
- Contacts section
- Interactions section
- And more...

### How Does It Work?

```
User clicks AI icon → Popup opens → User asks question or uploads doc
                            ↓
Backend fetches relevant guidance from database
                            ↓
Backend builds Gemini prompt with context + guidance + entity data
                            ↓
Gemini AI generates response (markdown or JSON for autofill)
                            ↓
Frontend displays answer or populates form fields
```

---

## 🏗️ Architecture Highlights

### New Components

**Database:**
- `ApplicationGuidance` table - Stores guidance markdown for sections

**Backend:**
- `ApplicationGuidanceManager` - Manages guidance retrieval
- `AiSectionAssistanceManager` - Orchestrates AI assistance
- `AiSectionAssistanceController` - API endpoints

**Frontend:**
- `AiSectionAssistanceComponent` - Reusable popup component
- `AiSectionAssistanceService` - API communication

### Key Technologies

- **AI Model:** Gemini 2.0 Flash (Vertex AI)
- **Storage:** Google Cloud Storage (GCS) for documents
- **Database:** PostgreSQL (new ApplicationGuidance table)
- **Framework:** .NET 8 + Angular 19
- **UI:** PrimeNG + Tailwind CSS

---

## 📅 Timeline

| Phase | Duration | Goal |
|-------|----------|------|
| **Sprint 1** | Weeks 1-2 | Foundation: Database, backend, basic frontend |
| **Sprint 2** | Weeks 3-4 | Integration: Complete UI, add to 3 sections |
| **Sprint 3** | Weeks 5-6 | Polish: UAT, optimization, expand to 6 sections |
| **Phase 2** | Future | Embeddings: Add semantic search capabilities |

**Total MVP Duration:** 6 weeks

---

## 👥 Team & Resources

### Required Team
- 1 Backend Developer (.NET) - 6 days
- 1 Frontend Developer (Angular) - 7.5 days
- 1 Content Creator (Guidance) - 1.5 days
- 1 QA Engineer - 1 day (UAT)

### Infrastructure Costs
- Database: Minimal (1 new table)
- AI API: ~$100/month (estimated 1000 requests/day)
- Storage: ~1MB for guidance content

---

## ✅ Success Criteria

### MVP Success Metrics

| Metric | Target |
|--------|--------|
| Response Time | < 5 seconds (95th percentile) |
| Error Rate | < 2% |
| User Adoption | > 20% of active users |
| Autofill Acceptance | > 40% |
| User Satisfaction | > 4/5 stars |

---

## 🚀 Next Actions

### This Week
1. ✅ Review all documentation
2. ✅ Approve architecture and approach
3. ✅ Create project tickets
4. ✅ Assign team members
5. ✅ Schedule kick-off meeting

### Next Week
1. ✅ Start Sprint 1 development
2. ✅ Set up database migrations
3. ✅ Begin guidance content creation

---

## 📋 Key Decisions Made

### ✅ Decision 1: Storage Strategy
**Create dedicated `ApplicationGuidance` table**
- Clear separation from AI prompts
- Better querying and management
- Easier to version and maintain

### ✅ Decision 2: Retrieval Strategy
**Phased approach: Direct DB queries first, embeddings later**
- Phase 1 (MVP): Direct database retrieval
- Phase 2 (Enhancement): Add vector embeddings for semantic search
- **Rationale:** Get to production faster, validate feature value first

### ✅ Decision 3: Component Design
**Single reusable component with configuration inputs**
- Easy to add to new sections
- Consistent UX across application
- Maintainable and testable

---

## ❓ Questions & Answers

### Q: Why not use existing AI assistant?
**A:** The AI assistant is for general chat. This feature is **context-specific** to each section with **pre-loaded guidance** and **autofill capabilities**.

### Q: Can we add this to any section?
**A:** Yes! The component is designed to be **universally reusable**. Just provide feature/section context and optional quick actions.

### Q: What about mobile devices?
**A:** The component is **fully responsive** and mobile-friendly with touch-optimized interactions.

### Q: How do we manage guidance content?
**A:** Phase 1: Database seed scripts. Phase 3: Admin UI for WYSIWYG editing.

### Q: What if Gemini gives wrong answers?
**A:** Responses are **guidance-backed** and **context-aware**. Quality improves with better guidance content and prompt tuning.

---

## 📞 Contact & Support

### For Questions About:

**Architecture & Technical Design:**
- See: [Full Recommendations](./RECOMMENDATIONS.md)
- Contact: Technical Lead / Architect

**Implementation Tasks & Timeline:**
- See: [Implementation Plan](./IMPLEMENTATION-PLAN.md)
- Contact: Project Manager / Scrum Master

**Business Value & Priorities:**
- See: [Executive Summary](./EXECUTIVE-SUMMARY.md)
- Contact: Product Owner

**Development Issues:**
- Contact: Development Team Lead

---

## 📎 Related Resources

### Internal Documents
- `.cursor/rules/angular-component-guidelines.mdc` - Component development standards
- `.cursor/rules/dotnet-implementation.mdc` - Backend implementation patterns
- Existing AI components: `src/app/features/ai/components/`

### External References
- [Gemini API Documentation](https://cloud.google.com/vertex-ai/docs/generative-ai/model-reference/gemini)
- [PrimeNG Components](https://primeng.org/)
- [Angular 19 Signals](https://angular.dev/guide/signals)

---

## 🔖 Document Versions

| Document | Version | Last Updated | Status |
|----------|---------|--------------|--------|
| Executive Summary | 1.0 | 2025-11-26 | ✅ Ready for Review |
| Full Recommendations | 1.0 | 2025-11-26 | ✅ Ready for Review |
| Implementation Plan | 1.0 | 2025-11-26 | ✅ Ready for Review |
| README (this file) | 1.0 | 2025-11-26 | ✅ Ready for Review |

---

## 📝 Changelog

### 2025-11-26 - Initial Documentation
- Created comprehensive recommendations
- Defined architecture and storage strategy
- Created detailed implementation plan
- Prepared for team review

---

**Status:** 📋 Awaiting approval to proceed to development

**Next Milestone:** Sprint 1 Kick-off (Week 1, Day 1)

---

*This documentation was created through comprehensive codebase analysis and architectural planning. All recommendations are based on existing system patterns and best practices.*

