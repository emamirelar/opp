# OpportunityScheduleManager Test Cases

**Manager:** `OpportunityScheduleManager`  
**Entity:** `OpportunitySchedule`, `ScheduleMilestone`  
**Test Count:** 15+  
**Priority:** P1 (High)  
**Created:** January 13, 2026

---

## Overview

Test cases for opportunity schedule management including high-level schedule generation, Work Breakdown Structure (WBS) creation, milestone tracking, and timeline visualization.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Schedule Generation | 5 | P1 |
| WBS Creation | 4 | P1 |
| Milestone Management | 3 | P1 |
| Timeline Validation | 3 | P1 |

---

## 1. Schedule Generation

### TC-OPP-SCH-F-001: Generate High-Level Schedule from Deliverables
**Priority:** P1  
**Test Steps:**
1. Create opportunity with 3 deliverables
2. Call `GenerateScheduleAsync(opportunityId)`
3. Verify schedule created

**Expected Results:**
- Schedule created with phases:
  - Development phase (current)
  - Implementation phases per deliverable
  - Closeout phase
- Duration estimated per phase
- Start/end dates calculated
- Dependencies identified

---

### TC-OPP-SCH-F-002: Include Development Phases in Timeline
**Priority:** P1  
**Test Steps:**
1. Generate schedule
2. Verify development phases included

**Expected Results:**
- Development phases visible:
  - Concept development (current)
  - Proposal preparation
  - Partner negotiations
  - Approval process
  - Mobilization
- Separate from implementation
- Clear transition point

---

### TC-OPP-SCH-F-003: Generate WBS from Deliverables
**Priority:** P1  
**Test Steps:**
1. Deliverables defined with sub-components
2. Generate WBS
3. Verify hierarchical structure

**Expected Results:**
- WBS shows hierarchy:
  - Level 1: Opportunity
  - Level 2: Deliverables (3)
  - Level 3: Work packages per deliverable
  - Level 4: Activities
- WBS codes assigned
- Visual representation available

---

### TC-OPP-SCH-F-004: Estimate Duration Based on Complexity
**Priority:** P1  
**Test Steps:**
1. Simple deliverable (DST complexity = 3)
2. Complex deliverable (DST complexity = 8)
3. Generate schedule
4. Verify duration estimation

**Expected Results:**
- Simple: 6 months estimated
- Complex: 18 months estimated
- Complexity factor applied
- Historical data referenced
- Confidence intervals provided

---

### TC-OPP-SCH-F-005: Consider Seasonal Factors
**Priority:** P2  
**Test Steps:**
1. Opportunity in monsoon-prone region
2. Generate schedule
3. Verify seasonal considerations

**Expected Results:**
- Monsoon season flagged
- Avoid scheduling during high-risk periods
- Buffer time added
- Alternative approaches suggested

---

## 2. Work Breakdown Structure (WBS)

### TC-OPP-SCH-WBS-001: Create Hierarchical WBS
**Priority:** P1  
**Test Steps:**
1. Generate WBS for opportunity
2. Verify hierarchy correct

**Expected Results:**
```
1.0 Water Infrastructure Initiative
  1.1 Infrastructure Development
    1.1.1 Site Preparation
    1.1.2 Construction
    1.1.3 Testing & Commissioning
  1.2 Capacity Building
    1.2.1 Training Programs
    1.2.2 Knowledge Transfer
  1.3 M&E Framework
    1.3.1 Baseline Study
    1.3.2 Ongoing Monitoring
    1.3.3 Final Evaluation
```

---

### TC-OPP-SCH-WBS-002: Assign WBS Codes
**Priority:** P1  
**Test Steps:**
1. Create WBS
2. Verify codes assigned consistently

**Expected Results:**
- Each element has unique code
- Codes reflect hierarchy
- Easy to reference
- Sortable and filterable

---

### TC-OPP-SCH-WBS-003: Link Budget to WBS Elements
**Priority:** P1  
**Test Steps:**
1. Create WBS
2. Allocate budget to WBS elements
3. Verify budget traceability

**Expected Results:**
- Each WBS element has budget
- Roll-up totals correct
- Budget accountability clear
- Can track spending by WBS code

---

### TC-OPP-SCH-WBS-004: Export WBS to Project Management Tools
**Priority:** P2  
**Test Steps:**
1. Create WBS
2. Export to MS Project format

**Expected Results:**
- WBS exported successfully
- Hierarchy preserved
- Dates included
- Can be imported to PM tools

---

## 3. Milestone Management

### TC-OPP-SCH-MIL-001: Define Key Milestones
**Priority:** P1  
**Test Steps:**
1. Generate schedule
2. Identify key milestones
3. Verify milestones tracked

**Expected Results:**
- Milestones identified:
  - Concept approval
  - Budget authorization
  - Partner agreement signed
  - Mobilization complete
  - Phase 1 completion
  - Project completion
- Milestone dates set
- Responsible parties assigned

---

### TC-OPP-SCH-MIL-002: Track Milestone Progress
**Priority:** P1  
**Test Steps:**
1. Milestones defined
2. Mark milestone as complete
3. Verify progress tracking

**Expected Results:**
- Milestone status updated
- Actual vs planned date tracked
- Delays flagged
- Impact on downstream milestones calculated

---

### TC-OPP-SCH-MIL-003: Critical Path Analysis
**Priority:** P2  
**Test Steps:**
1. Generate schedule with dependencies
2. Identify critical path
3. Verify critical activities highlighted

**Expected Results:**
- Critical path identified
- Non-critical activities have float
- Delays on critical path flagged
- What-if scenarios possible

---

## 4. Timeline Validation

### TC-OPP-SCH-VAL-001: Validate Start Before End Date
**Priority:** P1  
**Test Steps:**
1. Set StartDate = 2026-12-01
2. Set EndDate = 2026-01-01
3. Attempt to save

**Expected Results:**
- Validation error
- Error explains date logic
- Dates not saved

---

### TC-OPP-SCH-VAL-002: Warn on Unrealistic Duration
**Priority:** P1  
**Test Steps:**
1. Complex infrastructure project
2. Set 3-month duration
3. Verify warning

**Expected Results:**
- Warning displayed
- Historical data shown
- Similar projects took 12-18 months
- User can override with justification

---

### TC-OPP-SCH-VAL-003: Check Resource Availability
**Priority:** P2  
**Test Steps:**
1. Schedule requires 5 engineers
2. Only 2 available in timeframe
3. Verify resource conflict flagged

**Expected Results:**
- Resource constraint identified
- Alternative dates suggested
- Can adjust team size
- External resources suggested

---

## 5. Visualization

### TC-OPP-SCH-VIZ-001: Generate Gantt Chart
**Priority:** P2  
**Test Steps:**
1. Generate schedule
2. Create Gantt chart visualization

**Expected Results:**
- Gantt chart shows:
  - All activities
  - Durations
  - Dependencies
  - Critical path highlighted
  - Milestones marked
- Interactive (zoom, filter)

---

### TC-OPP-SCH-VIZ-002: Timeline View by Phase
**Priority:** P2  
**Test Steps:**
1. View schedule grouped by phase

**Expected Results:**
- Phases clearly delineated
- Phase durations visible
- Phase transitions marked
- Color-coded by phase

---

## Summary

**Total Test Cases:** 15+  
**High (P1):** 12  
**Medium (P2):** 5

**Execution Time:** ~5-6 minutes  
**Dependencies:** Opportunity, Deliverables, DST Profile

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `OpportunityScheduleManagerTests.cs`  
**Status:** ✅ Ready for Implementation
