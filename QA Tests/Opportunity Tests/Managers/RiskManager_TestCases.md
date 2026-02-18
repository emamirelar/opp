# RiskManager Test Cases

**Manager:** `RiskManager`  
**Entity:** `Risk`, `RiskRegister`  
**Test Count:** 15+  
**Priority:** P1 (High)  
**Created:** January 13, 2026

---

## Overview

Test cases for risk management including risk identification, assessment, mitigation planning, and register maintenance throughout opportunity lifecycle.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Risk Identification | 5 | P1 |
| Risk Assessment | 4 | P1 |
| Mitigation Planning | 3 | P1 |
| Risk Monitoring | 3 | P2 |

---

## 1. Risk Identification

### TC-OPP-RISK-F-001: Create Opportunity-Linked Risk Register
**Priority:** P1  
**Test Steps:**
1. Create opportunity
2. Call `CreateRiskRegisterAsync(opportunityId)`
3. Verify register created and linked

**Expected Results:**
- Risk register entity created
- Linked to opportunity
- Initially empty
- Ready for risk entries
- Single register through lifecycle

---

### TC-OPP-RISK-F-002: AI Suggests Risks from Similar Projects
**Priority:** P1  
**Test Steps:**
1. Generate DST profile
2. Call `SuggestRisksAsync(opportunityId)`
3. Verify risks suggested

**Expected Results:**
- Risks from similar projects identified:
  - "Monsoon delays" (from Bangladesh project)
  - "Partner capacity issues" (from Nepal project)
  - "Equipment import delays"
- Frequency/severity from historical data
- Can add to register with one click

---

### TC-OPP-RISK-F-003: Flag Country-Specific Risks
**Priority:** P1  
**Test Steps:**
1. Opportunity in fragile state
2. Check country risks

**Expected Results:**
- Country-specific risks flagged:
  - Political instability
  - Security concerns
  - Corruption risks
  - Currency fluctuation
- Based on country profile data

---

### TC-OPP-RISK-F-004: Add Risk Manually
**Priority:** P1  
**Test Steps:**
1. User identifies risk not suggested
2. Add to register manually

**Expected Results:**
- Risk added with:
  - Title
  - Description
  - Category (Strategic, Operational, Financial, etc.)
  - Probability (1-5)
  - Impact (1-5)
  - Risk score calculated
  - Mitigation strategy

---

### TC-OPP-RISK-F-005: Track When Risk Identified
**Priority:** P1  
**Test Steps:**
1. Add risk during development phase
2. Add risk during implementation phase
3. Query risk timeline

**Expected Results:**
- Each risk has:
  - IdentifiedDate
  - IdentifiedBy user
  - IdentifiedPhase (Development/Implementation)
- Timeline view available
- Can filter by phase

---

## 2. Risk Assessment

### TC-OPP-RISK-ASS-001: Calculate Risk Score
**Priority:** P1  
**Test Steps:**
1. Add risk with Probability = 4, Impact = 5
2. Verify risk score calculated

**Expected Results:**
- Risk score = Probability × Impact
- Score = 4 × 5 = 20
- Severity: Critical (score > 15)
- Color-coded: Red
- Ranking among other risks

---

### TC-OPP-RISK-ASS-002: Categorize Risk Severity
**Priority:** P1  
**Test Steps:**
1. Add risks with different scores
2. Verify severity categorization

**Expected Results:**
- Low (score 1-6): Green
- Medium (score 7-12): Yellow
- High (score 13-19): Orange
- Critical (score 20-25): Red
- Auto-categorized
- Can override with justification

---

### TC-OPP-RISK-ASS-003: Risk Heat Map
**Priority:** P2  
**Test Steps:**
1. Register has 10 risks
2. Generate heat map

**Expected Results:**
- 5×5 matrix
- Probability on Y-axis
- Impact on X-axis
- Risks plotted
- Visual prioritization

---

### TC-OPP-RISK-ASS-004: Residual Risk After Mitigation
**Priority:** P2  
**Test Steps:**
1. Initial risk score = 20
2. Mitigation reduces probability to 2
3. Calculate residual risk

**Expected Results:**
- Initial risk: 4×5 = 20 (Critical)
- Residual risk: 2×5 = 10 (Medium)
- Mitigation effectiveness shown
- Target risk level set

---

## 3. Mitigation Planning

### TC-OPP-RISK-MIT-001: Define Mitigation Strategy
**Priority:** P1  
**Test Steps:**
1. Add risk
2. Define mitigation strategy

**Expected Results:**
- Mitigation strategy captured:
  - Actions to reduce probability
  - Actions to reduce impact
  - Contingency plans
  - Responsible person
  - Target completion date
  - Budget required

---

### TC-OPP-RISK-MIT-002: Track Mitigation Action Status
**Priority:** P1  
**Test Steps:**
1. Mitigation actions defined
2. Mark actions as In Progress/Complete

**Expected Results:**
- Status tracking:
  - Not Started
  - In Progress
  - Completed
  - Overdue (flagged)
- Progress percentage
- Responsibility clear

---

### TC-OPP-RISK-MIT-003: Link Mitigation to Budget
**Priority:** P2  
**Test Steps:**
1. Mitigation requires $50K
2. Allocate budget to risk mitigation

**Expected Results:**
- Mitigation cost captured
- Budget allocated
- Can track actual costs
- Included in opportunity budget

---

## 4. Risk Monitoring

### TC-OPP-RISK-MON-001: Update Risk Status
**Priority:** P2  
**Test Steps:**
1. Risk status = Open
2. Mitigation completed, risk realized

**Expected Results:**
- Status options:
  - Open (active)
  - Mitigated (reduced)
  - Realized (occurred)
  - Closed (no longer relevant)
- Status history tracked
- Transitions documented

---

### TC-OPP-RISK-MON-002: Risk Register Version Control
**Priority:** P1  
**Test Steps:**
1. Initial register at development phase
2. Update during implementation
3. Query register history

**Expected Results:**
- Version at each opportunity stage
- Can view historical registers
- Compare versions
- Show evolution of risks

---

### TC-OPP-RISK-MON-003: Generate Risk Report
**Priority:** P2  
**Test Steps:**
1. Risk register with 15 risks
2. Generate report

**Expected Results:**
- PDF report with:
  - Summary statistics
  - High/critical risks highlighted
  - Mitigation status
  - Heat map
  - Timeline
  - Recommendations

---

## 5. Integration

### TC-OPP-RISK-INT-001: Link Risks to Decision Rationale
**Priority:** P1  
**Test Steps:**
1. DST identifies 3 critical risks
2. DOA holder references in Go decision

**Expected Results:**
- Risks linked to decision
- Mitigation plan required for approval
- Risk acceptance documented
- Accountability clear

---

### TC-OPP-RISK-INT-002: Risks Feed into Project Risk Register
**Priority:** P1  
**Test Steps:**
1. Opportunity converted to project
2. Verify risks transferred

**Expected Results:**
- Opportunity risks copied to project register
- Historical context preserved
- Development-phase risks flagged
- Single continuous register

---

## Summary

**Total Test Cases:** 15+  
**High (P1):** 12  
**Medium (P2):** 5

**Execution Time:** ~5 minutes  
**Dependencies:** Opportunity, DST Profile, Decision

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `RiskManagerTests.cs`  
**Status:** ✅ Ready for Implementation
