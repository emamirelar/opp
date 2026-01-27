# ResourcePlanManager Test Cases

**Manager:** `ResourcePlanManager`  
**Entity:** `ResourcePlan`, `ResourceRequirement`  
**Test Count:** 15+  
**Priority:** P1 (High)  
**Created:** January 13, 2026

---

## Overview

Test cases for resource planning including identification of development and implementation roles, personnel budgeting, and resource availability checking.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Role Identification | 5 | P1 |
| Personnel Budgeting | 4 | P1 |
| Resource Availability | 3 | P1 |
| Skills Matching | 3 | P2 |

---

## 1. Role Identification

### TC-OPP-RES-F-001: Identify Development Roles
**Priority:** P1  
**Test Steps:**
1. Create opportunity
2. Call `IdentifyDevelopmentRolesAsync(opportunityId)`
3. Verify roles identified

**Expected Results:**
- Development roles identified:
  - Opportunity Manager
  - Business Developer
  - Technical Advisor
  - Financial Analyst
  - Legal Reviewer
- FTE requirements estimated
- Duration for each role

---

### TC-OPP-RES-F-002: Identify Implementation Roles
**Priority:** P1  
**Test Steps:**
1. Opportunity with infrastructure deliverables
2. Identify implementation roles

**Expected Results:**
- Implementation roles identified:
  - Project Manager
  - Civil Engineers (3)
  - Procurement Specialist
  - M&E Specialist
  - Administrative Support
- FTE requirements calculated
- Skills required defined

---

### TC-OPP-RES-F-003: Identify Specialized Expertise Required
**Priority:** P1  
**Test Steps:**
1. Opportunity in fragile state
2. Identify roles
3. Verify specialized roles flagged

**Expected Results:**
- Specialized roles identified:
  - Security Advisor
  - Gender Advisor
  - Environmental Specialist
  - Conflict Sensitivity Expert
- Justification for each
- Availability checked

---

### TC-OPP-RES-F-004: Differentiate Core vs Support Personnel
**Priority:** P1  
**Test Steps:**
1. Generate resource plan
2. Categorize personnel

**Expected Results:**
- Core team (dedicated):
  - Project Manager (100%)
  - Lead Engineer (100%)
- Support personnel (partial):
  - Legal (10%)
  - Finance (15%)
  - HR (5%)
- Clear categorization

---

### TC-OPP-RES-F-005: Named vs Generic Resources
**Priority:** P2  
**Test Steps:**
1. Key positions filled by named individuals
2. Other positions generic placeholders

**Expected Results:**
- Named resources:
  - Project Manager: John Doe
  - Lead Engineer: Jane Smith
- Generic resources:
  - Engineer (2 positions)
  - Admin Support
- Can convert generic to named

---

## 2. Personnel Budgeting

### TC-OPP-RES-BUD-001: Calculate Personnel Costs
**Priority:** P1  
**Test Steps:**
1. Define resource requirements
2. Calculate personnel budget

**Expected Results:**
- Personnel costs calculated:
  - Salaries
  - Benefits
  - Travel and per diem
  - Training
- Total personnel budget
- Breakdown by role

---

### TC-OPP-RES-BUD-002: Apply Daily/Monthly Rates
**Priority:** P1  
**Test Steps:**
1. International expert (daily rate $800)
2. Local staff (monthly salary $3000)
3. Calculate costs

**Expected Results:**
- International: $800/day × 90 days = $72K
- Local: $3K/month × 18 months = $54K
- Correct rate application
- Currency handling

---

### TC-OPP-RES-BUD-003: Calculate Benefits and Overheads
**Priority:** P1  
**Test Steps:**
1. Base salary costs calculated
2. Add benefits and overheads

**Expected Results:**
- Base salary: $100K
- Benefits (30%): $30K
- Overhead (15%): $15K
- Total: $145K
- Breakdown visible

---

### TC-OPP-RES-BUD-004: Personnel Cost Over Time
**Priority:** P2  
**Test Steps:**
1. Generate personnel cost curve
2. Show costs by month

**Expected Results:**
- Monthly personnel costs charted
- Ramp-up period visible
- Peak staffing identified
- Demobilization planned

---

## 3. Resource Availability

### TC-OPP-RES-AVAIL-001: Check Resource Availability
**Priority:** P1  
**Test Steps:**
1. Require Project Manager for Jan-Jun 2027
2. Check availability

**Expected Results:**
- Available PMs listed
- Existing commitments shown
- Conflicts flagged
- Can reserve resource

---

### TC-OPP-RES-AVAIL-002: Handle Resource Conflicts
**Priority:** P1  
**Test Steps:**
1. Resource committed to another opportunity
2. Verify conflict detected

**Expected Results:**
- Conflict warning
- Alternative resources suggested
- Can adjust timeline
- Or request resource reallocation

---

### TC-OPP-RES-AVAIL-003: Reserve Resources Tentatively
**Priority:** P2  
**Test Steps:**
1. Opportunity not yet approved
2. Reserve resources tentatively

**Expected Results:**
- Tentative reservation created
- Expires if not confirmed
- Doesn't block hard commitments
- Can convert to firm booking

---

## 4. Skills Matching

### TC-OPP-RES-SKILL-001: Match Skills to Requirements
**Priority:** P2  
**Test Steps:**
1. Require civil engineer with bridge experience
2. Search for matching personnel

**Expected Results:**
- Candidates with bridge experience listed
- Relevance score provided
- Past project references
- Availability indicated

---

### TC-OPP-RES-SKILL-002: Identify Skills Gaps
**Priority:** P2  
**Test Steps:**
1. Require blockchain expertise
2. No internal expertise available

**Expected Results:**
- Gap identified
- Training options presented
- External hiring recommended
- Timeline implications shown

---

### TC-OPP-RES-SKILL-003: Development Needs Assessment
**Priority:** P2  
**Test Steps:**
1. Resource partially qualified
2. Identify development needs

**Expected Results:**
- Skills gap analysis
- Training recommendations
- Development timeline
- Cost of training

---

## 5. Reporting

### TC-OPP-RES-REP-001: Generate Resource Plan Report
**Priority:** P2  
**Test Steps:**
1. Create comprehensive resource plan
2. Generate report

**Expected Results:**
- PDF report with:
  - Org chart
  - Role descriptions
  - Personnel costs
  - Availability matrix
  - Skills matrix

---

### TC-OPP-RES-REP-002: Resource Utilization Forecast
**Priority:** P2  
**Test Steps:**
1. Multiple opportunities requiring resources
2. Generate utilization forecast

**Expected Results:**
- Utilization by role
- Over-/under-utilization identified
- Hiring needs projected
- Capacity planning supported

---

## Summary

**Total Test Cases:** 15+  
**High (P1):** 10  
**Medium (P2):** 7

**Execution Time:** ~5 minutes  
**Dependencies:** Opportunity, Personnel database, Skills database

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `ResourcePlanManagerTests.cs`  
**Status:** ✅ Ready for Implementation
