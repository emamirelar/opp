# GlobalIndicesManager Test Cases

**Manager:** `GlobalIndicesManager`  
**Entity:** `GlobalIndex`, `CountryIndex`  
**Test Count:** 15+  
**Priority:** P2 (Medium)  
**Created:** January 13, 2026

---

## Overview

Test cases for managing global country indices including periodic uploads, data replacement, historical tracking, and integration with business rules.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Data Upload | 5 | P2 |
| Historical Tracking | 3 | P2 |
| Business Rules Integration | 4 | P2 |
| Reporting | 3 | P2 |

---

## 1. Data Upload

### TC-OPP-GI-F-001: Upload New Global Index
**Priority:** P2  
**Test Steps:**
1. Prepare CSV with MVI scores for all countries
2. Call `UploadGlobalIndexAsync("MVI", file, effectiveDate)`
3. Verify data uploaded

**Expected Results:**
- All 193 countries updated
- MVI score stored per country
- EffectiveDate = specified date
- Previous values archived
- Upload log created

**Test Data:**
```csv
CountryCode,CountryName,MVIScore,Year
BD,Bangladesh,32.5,2025
NP,Nepal,28.7,2025
...
```

---

### TC-OPP-GI-F-002: Replace Outdated Index Data
**Priority:** P2  
**Test Steps:**
1. Existing MVI data from 2024
2. Upload 2025 data
3. Verify replacement

**Expected Results:**
- 2024 data moved to history
- 2025 data now current
- Can query both versions
- "Current" flag updated correctly

---

### TC-OPP-GI-F-003: Add New Index Field
**Priority:** P2  
**Test Steps:**
1. New index "Climate Risk Index" introduced
2. Add field to CountryProfile
3. Upload data for all countries

**Expected Results:**
- New field added to schema
- Data populated for all countries
- Historical records reflect null for past
- Documentation updated

---

### TC-OPP-GI-F-004: Retire Obsolete Index
**Priority:** P2  
**Test Steps:**
1. Index "XYZ" no longer relevant
2. Mark as retired
3. Verify handling

**Expected Results:**
- Index marked "Retired"
- RetiredDate recorded
- No longer shown in UI
- Historical data preserved
- Cannot upload new data

---

### TC-OPP-GI-F-005: Validate Upload Data Quality
**Priority:** P2  
**Test Steps:**
1. Upload file with missing countries
2. Verify validation

**Expected Results:**
- Missing countries identified
- Data quality report generated
- Can correct and re-upload
- Partial upload not allowed

---

## 2. Historical Tracking

### TC-OPP-GI-HIST-001: Maintain "As-At" Views
**Priority:** P2  
**Test Steps:**
1. MVI uploaded for 2024, 2025, 2026
2. Query country MVI as-at 2025-06-01

**Expected Results:**
- Returns 2025 MVI value
- Not the 2026 value
- Historical accuracy
- Supports reporting for specific periods

---

### TC-OPP-GI-HIST-002: Query Historical Index Data
**Priority:** P2  
**Test Steps:**
1. Multiple years of data
2. Query `GetCountryIndexHistoryAsync(countryId, "MVI")`

**Expected Results:**
- All historical values returned
- Chronological order
- Each with effective date
- Can chart trend over time

---

### TC-OPP-GI-HIST-003: Compare Index Across Years
**Priority:** P2  
**Test Steps:**
1. Compare Bangladesh MVI 2024 vs 2025

**Expected Results:**
- 2024: 31.2
- 2025: 32.5
- Change: +1.3 (improved)
- Trend analysis
- Context for current value

---

## 3. Business Rules Integration

### TC-OPP-GI-BR-001: Use MVI in DST Profiling
**Priority:** P2  
**Test Steps:**
1. Opportunity in low MVI country
2. Generate DST profile
3. Verify MVI considered

**Expected Results:**
- MVI score referenced in context evaluation
- Low MVI increases complexity score
- Specific risks flagged
- Mitigation recommendations

---

### TC-OPP-GI-BR-002: Identify Fragile States
**Priority:** P2  
**Test Steps:**
1. Country with Fragility Index > 90
2. Create opportunity in that country
3. Verify business rules applied

**Expected Results:**
- Fragile state flag shown
- Additional approvals required
- Security assessment mandatory
- Specialized personnel needed
- Enhanced monitoring

---

### TC-OPP-GI-BR-003: Use Corruption Index in Risk Assessment
**Priority:** P2  
**Test Steps:**
1. Country with high corruption perception
2. Generate risk register
3. Verify corruption risk added

**Expected Results:**
- Corruption risk auto-added
- Severity based on index
- Mitigation strategies suggested
- Due diligence heightened

---

### TC-OPP-GI-BR-004: Apply Thresholds for Reporting
**Priority:** P2  
**Test Steps:**
1. Report: "Countries with MVI < 30"
2. Generate report
3. Verify correct countries listed

**Expected Results:**
- Countries meeting criteria listed
- Opportunity count per country
- Total value of work
- Trend analysis
- Strategic insights

---

## 4. Reporting

### TC-OPP-GI-REP-001: Report Delivery by MVI Score
**Priority:** P2  
**Test Steps:**
1. Query: Opportunities in countries MVI < 25
2. Time period: 2025

**Expected Results:**
- List of qualifying opportunities
- Total count
- Total value
- Percentage of portfolio
- Year-over-year comparison

---

### TC-OPP-GI-REP-002: Index Dashboard
**Priority:** P2  
**Test Steps:**
1. View global indices dashboard

**Expected Results:**
- Summary of all indices
- Last update dates
- Coverage (% countries with data)
- Data quality indicators
- Upcoming updates flagged

---

### TC-OPP-GI-REP-003: Export Index Data
**Priority:** P2  
**Test Steps:**
1. Export all indices for all countries
2. Generate Excel file

**Expected Results:**
- Comprehensive export
- One sheet per index
- All historical data
- Metadata included
- Can be used for analysis

---

## 5. Configuration

### TC-OPP-GI-CONF-001: Define Index Metadata
**Priority:** P2  
**Test Steps:**
1. New index registered
2. Define metadata

**Expected Results:**
- Index metadata captured:
  - Name
  - Description
  - Data type (numeric, text, boolean)
  - Source organization
  - Update frequency
  - Interpretation guidance
  - Thresholds for alerts

---

### TC-OPP-GI-CONF-002: Set Alert Thresholds
**Priority:** P2  
**Test Steps:**
1. Configure: Alert if MVI < 25
2. Country updates to MVI = 24
3. Verify alert triggered

**Expected Results:**
- Alert triggered
- Relevant stakeholders notified
- Active opportunities in country flagged
- Review recommended

---

## Summary

**Total Test Cases:** 15+  
**Medium (P2):** 15

**Execution Time:** ~5 minutes  
**Dependencies:** Country, Opportunity, DST, Reporting

---

**Notes:**
- Indices managed centrally by HQ units
- Updates typically annual or semi-annual
- Critical for context-aware profiling
- Supports UNOPS reporting obligations

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `GlobalIndicesManagerTests.cs`  
**Status:** ✅ Ready for Implementation
