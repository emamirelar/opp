# AgreementService Test Cases

**Service:** `AgreementService`  
**Test Count:** 5+  
**Priority:** P2  
**Created:** January 13, 2026

---

## Overview

Partnership agreement service tests for document processing, term extraction, and integration.

---

## Test Cases

### TC-OPP-AGRSVC-001: Process Agreement Upload
**Priority:** P2  
**Test Steps:**
1. Receive PDF document
2. Extract text via OCR/AI
3. Parse terms
4. Store in database

**Expected Results:**
- Document processed successfully
- Text extraction accurate
- Terms identified correctly
- Metadata stored

---

### TC-OPP-AGRSVC-002: Extract Key Terms with AI
**Priority:** P2  
**Test Steps:**
1. Call AI service with document text
2. Identify geography, scope, pricing
3. Structure and validate

**Expected Results:**
- Key terms extracted
- Confidence scores provided
- Structured data created
- User can verify/correct

---

### TC-OPP-AGRSVC-003: Match Agreement to Partner
**Priority:** P2  
**Test Steps:**
1. Analyze document for partner name
2. Search partner database
3. Suggest matches

**Expected Results:**
- Partner identified (if exists)
- Fuzzy matching applied
- Multiple suggestions if ambiguous
- Can create new partner if needed

---

### TC-OPP-AGRSVC-004: Validate Opportunity Against Agreement
**Priority:** P2  
**Test Steps:**
1. Opportunity linked to agreement
2. Validate geography, scope, pricing
3. Flag violations

**Expected Results:**
- Geography validated
- Scope checked
- Pricing terms verified
- Violations clearly flagged

---

### TC-OPP-AGRSVC-005: Track Agreement Utilization
**Priority:** P2  
**Test Steps:**
1. Sum all opportunities under agreement
2. Calculate utilization %
3. Alert when approaching cap

**Expected Results:**
- Total utilization calculated
- Percentage of agreement value
- Warning at 80% utilization
- Alert at 95%

---

**Status:** ✅ Ready for Implementation
