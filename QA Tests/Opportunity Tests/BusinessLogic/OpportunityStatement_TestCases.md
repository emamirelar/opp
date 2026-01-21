# OpportunityStatement Test Cases

**Component:** Opportunity Statement Business Logic  
**Test Count:** 20+  
**Priority:** P1-P2 (High/Medium)  
**Created:** January 13, 2026

---

## Overview

Test cases for opportunity statement generation, template management, pre-population, narrative generation, and concept note creation.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Template Management | 5 | P1 |
| Pre-Population | 6 | P1 |
| Narrative Generation | 5 | P1 |
| Concept Note | 4 | P2 |

---

## 1. Template Management (P1)

### TC-OPP-STMT-TMP-001: Load Standard Template
**Priority:** P1  
**Expected Results:**
- Template with standard sections:
  - Executive Summary
  - Background and Context
  - Objectives
  - Approach and Methodology
  - Deliverables
  - Budget
  - Timeline
  - Risk Assessment
  - M&E Framework

---

### TC-OPP-STMT-TMP-002: Sector-Specific Templates
**Priority:** P1  
**Expected Results:**
- Infrastructure template emphasizes technical aspects
- Capacity building focuses on training
- Procurement highlights supply chain
- Template auto-selected based on opportunity type

---

### TC-OPP-STMT-TMP-003: Custom Template Creation
**Priority:** P2  
**Expected Results:**
- User can create custom sections
- Section order configurable
- Save as organization template
- Share with team

---

### TC-OPP-STMT-TMP-004: Template Versioning
**Priority:** P2  
**Expected Results:**
- Templates versioned
- Can revert to previous version
- Changes tracked
- Active version marked

---

### TC-OPP-STMT-TMP-005: Template Validation
**Priority:** P1  
**Expected Results:**
- Required sections enforced
- Section completeness checked
- Warnings for missing content
- Blocks submission if incomplete

---

## 2. Pre-Population (P1)

### TC-OPP-STMT-POP-001: Auto-Fill From Opportunity Data
**Priority:** P1  
**Expected Results:**
```
- Title → Opportunity.Name
- Budget → Opportunity.EstimatedValue formatted
- Timeline → Opportunity.StartDate to EndDate
- Countries → List of countries
- Partners → Partner names and roles
- Deliverables → Deliverables list
```

---

### TC-OPP-STMT-POP-002: Import From DST Profile
**Priority:** P1  
**Expected Results:**
- Context section from DST Parameter 4
- Risk section from DST risk assessment
- Complexity discussion from DST complexity score
- Recommendations incorporated

---

### TC-OPP-STMT-POP-003: Import From Document Extraction
**Priority:** P1  
**Expected Results:**
- If concept note uploaded, use extracted text
- Background from document
- Objectives from document
- Methodology from document
- User can edit/refine

---

### TC-OPP-STMT-POP-004: Import From Partnership Agreement
**Priority:** P2  
**Expected Results:**
- Agreement terms referenced
- Scope of work from agreement
- Pricing terms included
- Agreement number cited

---

### TC-OPP-STMT-POP-005: Merge Multiple Sources
**Priority:** P1  
**Expected Results:**
- Opportunity data (facts)
- DST profile (analysis)
- Document extraction (narrative)
- User entries (customization)
- Intelligent merge without duplication

---

### TC-OPP-STMT-POP-006: Handle Missing Data
**Priority:** P1  
**Expected Results:**
- Missing sections highlighted
- Placeholder text shown
- Prompts for required info
- Can generate partial statement

---

## 3. Narrative Generation (P1)

### TC-OPP-STMT-NAR-001: Generate Executive Summary
**Priority:** P1  
**Expected Results:**
- AI generates 250-word summary
- Includes:
  - Opportunity overview
  - Key objectives
  - Budget and timeline
  - Expected impact
- Professional tone
- User can edit

---

### TC-OPP-STMT-NAR-002: Generate Background Section
**Priority:** P1  
**Expected Results:**
- Country context from profiles
- Problem statement
- UNOPS value proposition
- Partner landscape
- 500-750 words
- Citations to data sources

---

### TC-OPP-STMT-NAR-003: Generate Methodology Section
**Priority:** P1  
**Expected Results:**
- Approach based on deliverables
- Technical methodology
- Implementation phases
- Quality assurance
- UNOPS best practices referenced

---

### TC-OPP-STMT-NAR-004: Generate Risk Section
**Priority:** P1  
**Expected Results:**
- Risks from risk register
- Mitigation strategies
- Risk ownership
- Monitoring approach
- Formatted as table or narrative

---

### TC-OPP-STMT-NAR-005: Maintain Consistency
**Priority:** P1  
**Expected Results:**
- Consistent terminology throughout
- No contradictions
- Budget figures match everywhere
- Timeline consistent across sections
- Validation checks run

---

## 4. Concept Note Creation (P2)

### TC-OPP-STMT-CN-001: Generate Partner-Facing Concept Note
**Priority:** P2  
**Expected Results:**
- Simplified language
- Partner-focused benefits
- Less internal jargon
- Emphasize outcomes
- Professional formatting

---

### TC-OPP-STMT-CN-002: Tailor to Partner Format
**Priority:** P2  
**Expected Results:**
- World Bank format option
- UN agency format option
- Government format option
- Format adapts to partner preference

---

### TC-OPP-STMT-CN-003: Include Visuals
**Priority:** P2  
**Expected Results:**
- Timeline chart
- Budget pie chart
- Geographic map
- Logo placement
- Professional layout

---

### TC-OPP-STMT-CN-004: Export Formats
**Priority:** P2  
**Expected Results:**
- PDF (primary)
- Word (editable)
- PowerPoint (presentation)
- HTML (web)
- All formats maintain formatting

---

## 5. Collaboration (P1)

### TC-OPP-STMT-COL-001: Real-Time Co-Editing
**Priority:** P1  
**Expected Results:**
- Multiple users edit simultaneously
- Changes visible in real-time
- No conflicts
- Auto-save every 30 seconds
- User avatars show who's editing

---

### TC-OPP-STMT-COL-002: Comments and Suggestions
**Priority:** P1  
**Expected Results:**
- Can comment on any section
- Suggest edits
- @mention colleagues
- Comments thread
- Resolve when addressed

---

### TC-OPP-STMT-COL-003: Review Workflow
**Priority:** P1  
**Expected Results:**
- Submit for review
- Reviewers notified
- Track review status
- Approve/request changes
- Version at each review cycle

---

## 6. Version Control (P1)

### TC-OPP-STMT-VER-001: Auto-Save Versions
**Priority:** P1  
**Expected Results:**
- Version saved every significant change
- Version number incremented
- Change summary auto-generated
- Can restore any version

---

### TC-OPP-STMT-VER-002: Compare Versions
**Priority:** P2  
**Expected Results:**
- Side-by-side comparison
- Additions highlighted green
- Deletions shown in red
- Unchanged text dimmed
- Can merge changes

---

### TC-OPP-STMT-VER-003: Lock Final Version
**Priority:** P1  
**Expected Results:**
- Final version locked at approval
- Snapshot preserved
- Linked to decision record
- Cannot be modified
- Can create new version if needed

---

## Summary

**Total Test Cases:** 20+  
**High (P1):** 15  
**Medium (P2):** 7

**Execution Time:** ~6-8 minutes  
**Dependencies:** Opportunity, DST, Templates, AI services

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `OpportunityStatementTests.cs`  
**Status:** ✅ Ready for Implementation
