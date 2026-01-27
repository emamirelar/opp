# AgreementLibrary Test Cases

**Component:** Partnership Agreement Library Logic  
**Test Count:** 20+  
**Priority:** P1-P2 (High/Medium)  
**Created:** January 13, 2026

---

## Overview

Test cases for partnership agreement storage, metadata extraction, key terms identification, agreement linkage, and pre-population logic.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Agreement Storage | 5 | P1 |
| Terms Extraction | 6 | P1 |
| Linkage | 4 | P1 |
| Pre-Population | 5 | P1 |

---

## 1. Agreement Storage (P1)

### TC-OPP-AGR-ST-001: Upload Partnership Agreement
**Priority:** P1  
**Test Steps:**
1. Upload signed agreement PDF
2. Verify storage and metadata

**Expected Results:**
- File stored securely
- Metadata extracted:
  - Partner name
  - Agreement type
  - Start/end dates
  - Value/amount
  - Geography
- Full-text searchable

---

### TC-OPP-AGR-ST-002: Link Agreement to Partner
**Priority:** P1  
**Test Steps:**
1. Upload agreement
2. Identify partner
3. Link agreement to partner record

**Expected Results:**
- Partner automatically identified from text
- Or manual linking
- Agreement visible in partner profile
- Can have multiple agreements per partner

---

### TC-OPP-AGR-ST-003: Agreement Versioning
**Priority:** P1  
**Test Steps:**
1. Original agreement uploaded
2. Amendment uploaded
3. Track versions

**Expected Results:**
- Version 1: Original
- Version 2: Amendment 1
- Version history maintained
- Current version marked
- Can view all versions

---

### TC-OPP-AGR-ST-004: Agreement Expiration Tracking
**Priority:** P1  
**Test Steps:**
1. Agreement with end date
2. Track expiration

**Expected Results:**
- Expiration date flagged
- Warning 90 days before
- Notification 30 days before
- Marked as expired after date
- Cannot link to new opportunities after expiration

---

### TC-OPP-AGR-ST-005: Agreement Search and Retrieval
**Priority:** P2  
**Test Steps:**
1. Search for agreements by partner, geography, type
2. Verify results

**Expected Results:**
- Full-text search
- Filter by metadata
- Sort by date, value
- Quick access to documents

---

## 2. Terms Extraction (P1)

### TC-OPP-AGR-EXT-001: Extract Geographic Scope
**Priority:** P1  
**Test Steps:**
1. Agreement states: "Valid for Bangladesh and Nepal"
2. Extract geography

**Expected Results:**
- Countries: [BD, NP]
- Mapped to country entities
- Geographic constraint recorded
- Visible when linking to opportunities

---

### TC-OPP-AGR-EXT-002: Extract Scope of Work
**Priority:** P1  
**Test Steps:**
1. Agreement defines eligible activities
2. Extract scope

**Expected Results:**
- Activity categories extracted
- Keywords identified
- Eligible/ineligible activities flagged
- Used to validate opportunity alignment

---

### TC-OPP-AGR-EXT-003: Extract Pricing Terms
**Priority:** P1  
**Test Steps:**
1. Agreement specifies fee structure
2. Extract pricing

**Expected Results:**
- Fee percentage: 8%
- Cost recovery terms
- Eligible cost categories
- Any caps or limits
- Used in budget calculations

---

### TC-OPP-AGR-EXT-004: Extract Financial Terms
**Priority:** P1  
**Test Steps:**
1. Agreement value, payment terms
2. Extract financial details

**Expected Results:**
- Total agreement value
- Payment schedule
- Currency
- Financial reporting requirements

---

### TC-OPP-AGR-EXT-005: Extract Validity Period
**Priority:** P1  
**Test Steps:**
1. Agreement dates
2. Extract and track

**Expected Results:**
- Start date
- End date
- Renewal provisions
- Notice period for termination

---

### TC-OPP-AGR-EXT-006: Extract Key Obligations
**Priority:** P2  
**Test Steps:**
1. Agreement lists UNOPS obligations
2. Extract as checklist

**Expected Results:**
- Obligations listed
- Categorized (reporting, quality, etc.)
- Linked to compliance tracking
- Can assign responsibility

---

## 3. Linkage (P1)

### TC-OPP-AGR-LINK-001: Link Agreement to Opportunity
**Priority:** P1  
**Test Steps:**
1. Create opportunity under partnership
2. Link to agreement
3. Verify linkage

**Expected Results:**
- Agreement selected from list
- Agreement terms visible
- Geographic scope validated
- Fee structure applied

---

### TC-OPP-AGR-LINK-002: Validate Opportunity Against Agreement
**Priority:** P1  
**Test Steps:**
1. Opportunity in country not covered by agreement
2. Verify validation warning

**Expected Results:**
- Geography mismatch flagged
- Warning displayed
- Can proceed with justification
- Or select different agreement

---

### TC-OPP-AGR-LINK-003: Multiple Agreements for One Opportunity
**Priority:** P2  
**Test Steps:**
1. Opportunity spans multiple partnerships
2. Link to multiple agreements

**Expected Results:**
- Can link multiple agreements
- Terms of each applied
- Conflicts flagged
- Most restrictive terms prevail

---

### TC-OPP-AGR-LINK-004: Agreement History
**Priority:** P2  
**Test Steps:**
1. View all opportunities under an agreement
2. Verify tracking

**Expected Results:**
- List of linked opportunities
- Total value under agreement
- Utilization percentage
- Timeline of opportunities

---

## 4. Pre-Population (P1)

### TC-OPP-AGR-POP-001: Pre-Fill Geography from Agreement
**Priority:** P1  
**Test Steps:**
1. Select agreement with Bangladesh scope
2. Create opportunity
3. Verify geography pre-filled

**Expected Results:**
- PrimaryCountry = Bangladesh
- Secondary countries = Nepal (if in agreement)
- Geographic constraint applied
- Can override if needed

---

### TC-OPP-AGR-POP-002: Pre-Fill Fee Structure
**Priority:** P1  
**Test Steps:**
1. Agreement specifies 8% fee
2. Create opportunity and budget
3. Verify fee applied

**Expected Results:**
- Fee percentage = 8%
- Applied to budget calculations
- Source = Partnership Agreement #123
- Can see fee justification

---

### TC-OPP-AGR-POP-003: Pre-Fill Scope Categories
**Priority:** P1  
**Test Steps:**
1. Agreement limits to infrastructure
2. Create opportunity
3. Verify scope constraints

**Expected Results:**
- Opportunity type limited to eligible categories
- Warning if selecting ineligible category
- Agreement reference shown

---

### TC-OPP-AGR-POP-004: Pre-Fill Partner Information
**Priority:** P1  
**Test Steps:**
1. Agreement with World Bank
2. Create opportunity
3. Verify partner pre-filled

**Expected Results:**
- Primary partner = World Bank
- Partner role pre-defined
- Contact information populated
- Agreement referenced

---

### TC-OPP-AGR-POP-005: Apply Agreement Templates
**Priority:** P2  
**Test Steps:**
1. Agreement has standard reporting format
2. Apply to opportunity
3. Verify template used

**Expected Results:**
- Reporting template applied
- Required sections included
- Format matches agreement
- Reduces manual setup

---

## 5. Validation (P1)

### TC-OPP-AGR-VAL-001: Validate Opportunity Value Against Agreement
**Priority:** P1  
**Test Steps:**
1. Agreement cap: $5M
2. Opportunity: $6M
3. Verify validation

**Expected Results:**
- Exceeds agreement cap flagged
- Warning displayed
- Can request amendment
- Or split into multiple opportunities

---

### TC-OPP-AGR-VAL-002: Validate Timeline Against Agreement
**Priority:** P1  
**Test Steps:**
1. Agreement expires Dec 2027
2. Opportunity end date: Jun 2028
3. Verify warning

**Expected Results:**
- Extends beyond agreement flagged
- Warning shown
- Must complete before expiration
- Or seek extension/new agreement

---

### TC-OPP-AGR-VAL-003: Validate Pricing Terms
**Priority:** P1  
**Test Steps:**
1. Agreement allows 8% fee
2. Opportunity budget uses 12% fee
3. Verify validation error

**Expected Results:**
- Fee mismatch detected
- Error prevents submission
- Must adjust to 8%
- Or get agreement amendment

---

## Integration Tests

### TC-OPP-AGR-INT-001: End-to-End Agreement Usage
**Priority:** P1  
**Test Steps:**
1. Upload agreement
2. Extract terms
3. Create opportunity
4. Pre-populate from agreement
5. Validate compliance

**Expected Results:**
- Seamless flow
- Time savings from pre-population
- Compliance ensured
- Audit trail maintained

---

### TC-OPP-AGR-INT-002: Agreement Portfolio View
**Priority:** P2  
**Test Steps:**
1. View all agreements for an organization
2. See utilization across all

**Expected Results:**
- List of agreements
- Total value available
- Value utilized
- Remaining capacity
- Expiration status

---

## Summary

**Total Test Cases:** 20+  
**High (P1):** 16  
**Medium (P2):** 6

**Execution Time:** ~6-8 minutes  
**Dependencies:** Partner, Opportunity, Document storage

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `AgreementLibraryTests.cs`  
**Status:** ✅ Ready for Implementation
