# OpportunityBudgetManager Test Cases

**Manager:** `OpportunityBudgetManager`  
**Entity:** `OpportunityBudget`  
**Test Count:** 20+  
**Priority:** P1 (High)  
**Created:** January 13, 2026

---

## Overview

Test cases for opportunity budget management including high-level budget generation, fee calculations, spend rate visualization, and development cost tracking.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Budget Generation | 6 | P1 |
| Fee Calculations | 4 | P1 |
| Cost Segregation | 4 | P1 |
| Validation | 3 | P1 |
| Reporting | 3 | P2 |

---

## 1. Budget Generation

### TC-OPP-BUD-F-001: Generate High-Level Budget from Opportunity
**Priority:** P1  
**Test Steps:**
1. Create opportunity with deliverables and resources
2. Call `GenerateHighLevelBudgetAsync(opportunityId)`
3. Verify budget created with cost breakdown

**Expected Results:**
- Personnel costs calculated
- Non-personnel costs estimated
- Equipment/supplies included
- Overhead calculated
- Total budget accurate

---

### TC-OPP-BUD-F-002: Apply Default Fee Percentage
**Priority:** P1  
**Test Steps:**
1. Create opportunity without pre-agreed fee
2. Generate budget
3. Verify default fee applied (e.g., 10%)

**Expected Results:**
- Default fee percentage retrieved from settings
- Applied to direct costs
- Fee amount calculated correctly
- Total includes fee

---

### TC-OPP-BUD-F-003: Use Pre-Agreed Fee from Partnership Agreement
**Priority:** P1  
**Test Steps:**
1. Create opportunity linked to partnership agreement with 8% fee
2. Generate budget
3. Verify agreement fee used instead of default

**Expected Results:**
- Fee from agreement used
- Override documented
- Source of fee visible in budget

---

### TC-OPP-BUD-F-004: Budget by Deliverable
**Priority:** P1  
**Test Steps:**
1. Create opportunity with 3 deliverables
2. Generate budget
3. Verify costs allocated to each deliverable

**Expected Results:**
- Costs broken down by deliverable
- Deliverable 1: $X
- Deliverable 2: $Y
- Deliverable 3: $Z
- Total = sum of deliverables

---

### TC-OPP-BUD-F-005: Budget by Cost Category
**Priority:** P1  
**Test Steps:**
1. Generate budget
2. Verify cost categorization

**Expected Results:**
- Personnel: $X (40%)
- Non-personnel: $Y (35%)
- Equipment: $Z (15%)
- Fee: $W (10%)
- Clear breakdown visible

---

### TC-OPP-BUD-F-006: Multi-Currency Budget
**Priority:** P1  
**Test Steps:**
1. Create opportunity with deliverables in different countries/currencies
2. Generate budget
3. Verify currency handling

**Expected Results:**
- Costs in original currencies preserved
- Conversion to primary currency
- Exchange rates documented
- Date of rates recorded

---

## 2. Fee Calculations

### TC-OPP-BUD-FEE-001: Calculate Fee on Direct Costs Only
**Priority:** P1  
**Test Steps:**
1. Budget has direct costs $900K, indirect costs $100K
2. Apply 10% fee
3. Verify fee calculated on direct costs only

**Expected Results:**
- Fee base = $900K (direct costs)
- Fee = $90K (10% of $900K)
- Total budget = $1.09M
- Indirect costs not double-counted

---

### TC-OPP-BUD-FEE-002: Tiered Fee Structure
**Priority:** P2  
**Test Steps:**
1. Budget > $5M gets 8% fee
2. Budget $1M-$5M gets 10% fee
3. Verify appropriate tier applied

**Expected Results:**
- Budget of $6M: 8% fee = $480K
- Budget of $3M: 10% fee = $300K
- Tier thresholds correctly applied

---

### TC-OPP-BUD-FEE-003: Fee Cap
**Priority:** P2  
**Test Steps:**
1. Very large budget ($50M)
2. Fee has cap of $3M
3. Verify cap enforced

**Expected Results:**
- Calculated fee would be $5M (10%)
- Capped at $3M
- Cap documented in budget

---

### TC-OPP-BUD-FEE-004: Zero Fee for Certain Partners
**Priority:** P2  
**Test Steps:**
1. Partner agreement specifies no fee
2. Generate budget
3. Verify no fee charged

**Expected Results:**
- Fee = $0
- Rationale documented
- Approved by appropriate authority

---

## 3. Development Cost Segregation

### TC-OPP-BUD-SEG-001: Separate Development from Implementation Costs
**Priority:** P1  
**Test Steps:**
1. Generate budget
2. Verify development phase costs separated

**Expected Results:**
- Development costs (unfunded): $100K
  - Concept development
  - Stakeholder consultations
  - Proposal writing
- Implementation costs (funded): $2.4M
  - Actual project delivery
- Clear segregation visible

---

### TC-OPP-BUD-SEG-002: Track Development Cost Actuals
**Priority:** P1  
**Test Steps:**
1. Development costs budgeted at $100K
2. Team logs $75K of development work
3. Query actual vs budget

**Expected Results:**
- Budget: $100K
- Actual: $75K
- Variance: -$25K (under budget)
- Utilization: 75%

---

### TC-OPP-BUD-SEG-003: Recover Development Costs
**Priority:** P2  
**Test Steps:**
1. Development costs of $100K
2. Opportunity approved
3. Recover costs from implementation budget

**Expected Results:**
- Development costs transferred to project
- Clear accounting of recovery
- Total project cost increased by $100K

---

### TC-OPP-BUD-SEG-004: Write Off Development Costs (No-Go)
**Priority:** P2  
**Test Steps:**
1. Opportunity declined (No-Go decision)
2. Development costs of $100K incurred
3. Verify costs written off

**Expected Results:**
- Costs marked as "Written off"
- Charged to appropriate cost center
- Lessons learned captured
- Impact on org unit budget

---

## 4. Spend Rate Visualization

### TC-OPP-BUD-VIZ-001: Generate Spend Rate Over Time
**Priority:** P1  
**Test Steps:**
1. Create 18-month implementation plan
2. Generate spend rate visualization
3. Verify S-curve or linear distribution

**Expected Results:**
- Monthly spend rate calculated
- Visual representation (chart/graph)
- Peak spending months identified
- Cash flow implications clear

---

### TC-OPP-BUD-VIZ-002: Adjust Spend Rate by Phase
**Priority:** P2  
**Test Steps:**
1. Project has 3 phases with different intensities
2. Generate spend rate
3. Verify phase-adjusted distribution

**Expected Results:**
- Phase 1 (Setup): 15% of budget
- Phase 2 (Implementation): 70% of budget
- Phase 3 (Closeout): 15% of budget
- Realistic distribution

---

### TC-OPP-BUD-VIZ-003: Compare Planned vs Actual Spend
**Priority:** P2  
**Test Steps:**
1. Planned spend rate defined
2. Track actual spend
3. Generate comparison chart

**Expected Results:**
- Planned vs Actual visible
- Variances highlighted
- Trend analysis
- Forecasting to completion

---

## 5. Validation

### TC-OPP-BUD-VAL-001: Budget Total Must Match Estimated Value
**Priority:** P1  
**Test Steps:**
1. Opportunity estimated value = $2.5M
2. Generated budget total = $2.7M
3. Verify validation warning

**Expected Results:**
- Warning displayed
- User can adjust estimate or budget
- Significant variances flagged
- Approval required for mismatches

---

### TC-OPP-BUD-VAL-002: All Deliverables Must Have Budget
**Priority:** P1  
**Test Steps:**
1. Create 3 deliverables
2. Budget only 2 of them
3. Attempt to save budget

**Expected Results:**
- Validation error
- Lists deliverable without budget
- Cannot save incomplete budget

---

### TC-OPP-BUD-VAL-003: Budget Cannot Be Negative
**Priority:** P1  
**Test Steps:**
1. Attempt to set negative cost
2. Verify validation error

**Expected Results:**
- Validation prevents negative values
- Error message clear
- No negative costs allowed

---

## 6. Reporting

### TC-OPP-BUD-REP-001: Generate Budget Summary Report
**Priority:** P2  
**Test Steps:**
1. Generate comprehensive budget
2. Create summary report

**Expected Results:**
- PDF report with:
  - Total budget
  - Cost breakdown
  - Deliverable costs
  - Fee calculation
  - Assumptions
  - Spend rate chart

---

### TC-OPP-BUD-REP-002: Export Budget to Excel
**Priority:** P2  
**Test Steps:**
1. Generate budget
2. Export to Excel

**Expected Results:**
- Excel file with multiple sheets
- Detailed cost breakdown
- Formulas intact
- Can be modified offline

---

### TC-OPP-BUD-REP-003: Budget Comparison Report
**Priority:** P2  
**Test Steps:**
1. Compare budgets of 3 opportunities
2. Generate comparison

**Expected Results:**
- Side-by-side comparison
- Efficiency metrics
- Cost per beneficiary
- Helps prioritization

---

## Summary

**Total Test Cases:** 20+  
**High (P1):** 15  
**Medium (P2):** 7

**Execution Time:** ~6-8 minutes  
**Dependencies:** Opportunity, Deliverables, Partnership Agreements

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `OpportunityBudgetManagerTests.cs`  
**Status:** ✅ Ready for Implementation
