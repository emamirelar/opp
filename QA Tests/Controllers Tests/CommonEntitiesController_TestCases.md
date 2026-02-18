# CommonEntitiesController Test Cases

**Controller**: `UNOPS.PAO.UNOPSPresentation/Controllers/CommonEntitiesController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 25  

---

## Overview

The CommonEntitiesController provides access to shared/lookup entities:
- Status values
- Types and classifications
- Reference data
- Common dropdowns

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Status Values | 6 | P0 |
| Types/Classifications | 8 | P1 |
| Reference Data | 6 | P1 |
| Caching | 3 | P2 |
| Authorization | 2 | P0 |

---

## P0 - Critical Tests

### TC-CEC-001: Get partner statuses
**Description**: List partner status values  
**Test Steps**:
1. Call `GET /api/common/partner-statuses`
**Expected Result**: Status list

### TC-CEC-002: Get contact statuses
**Description**: List contact status values  
**Test Steps**:
1. Call `GET /api/common/contact-statuses`
**Expected Result**: Status list

### TC-CEC-003: Get interaction types
**Description**: List interaction types  
**Test Steps**:
1. Call `GET /api/common/interaction-types`
**Expected Result**: Type list

### TC-CEC-004: Get document types
**Description**: List document types  
**Test Steps**:
1. Call `GET /api/common/document-types`
**Expected Result**: Type list

### TC-CEC-005: Get workflow statuses
**Description**: List workflow statuses  
**Test Steps**:
1. Call `GET /api/common/workflow-statuses`
**Expected Result**: Status list

### TC-CEC-006: Get entity statuses generic
**Description**: Generic status by entity  
**Test Steps**:
1. Call `GET /api/common/statuses/{entityType}`
**Expected Result**: Status list

---

## P1 - High Priority Tests

### TC-CEC-007: Get partner types
**Description**: List partner types  
**Test Steps**:
1. Call `GET /api/common/partner-types`
**Expected Result**: Type list

### TC-CEC-008: Get contact roles
**Description**: List contact roles  
**Test Steps**:
1. Call `GET /api/common/contact-roles`
**Expected Result**: Role list

### TC-CEC-009: Get org unit types
**Description**: List org unit types  
**Test Steps**:
1. Call `GET /api/common/org-unit-types`
**Expected Result**: Type list

### TC-CEC-010: Get engagement types
**Description**: List engagement types  
**Test Steps**:
1. Call `GET /api/common/engagement-types`
**Expected Result**: Type list

### TC-CEC-011: Get priority levels
**Description**: List priorities  
**Test Steps**:
1. Call `GET /api/common/priorities`
**Expected Result**: Priority list

### TC-CEC-012: Get currencies
**Description**: List currencies  
**Test Steps**:
1. Call `GET /api/common/currencies`
**Expected Result**: Currency list

### TC-CEC-013: Get languages
**Description**: List supported languages  
**Test Steps**:
1. Call `GET /api/common/languages`
**Expected Result**: Language list

### TC-CEC-014: Get timezones
**Description**: List timezones  
**Test Steps**:
1. Call `GET /api/common/timezones`
**Expected Result**: Timezone list

### TC-CEC-015: Get countries
**Description**: List countries (reference)  
**Test Steps**:
1. Call `GET /api/common/countries`
**Expected Result**: Country list

### TC-CEC-016: Get regions
**Description**: List regions  
**Test Steps**:
1. Call `GET /api/common/regions`
**Expected Result**: Region list

### TC-CEC-017: Get date formats
**Description**: List date formats  
**Test Steps**:
1. Call `GET /api/common/date-formats`
**Expected Result**: Format list

### TC-CEC-018: Get number formats
**Description**: List number formats  
**Test Steps**:
1. Call `GET /api/common/number-formats`
**Expected Result**: Format list

### TC-CEC-019: Get all lookup data
**Description**: Combined lookup data  
**Test Steps**:
1. Call `GET /api/common/all`
**Expected Result**: All common data

### TC-CEC-020: Filter lookup by locale
**Description**: Localized values  
**Test Steps**:
1. Call with locale=fr
**Expected Result**: French labels

---

## Caching Tests

### TC-CEC-C001: Response cached
**Description**: Verify caching  
**Test Steps**:
1. Call twice
2. Verify cache headers
**Expected Result**: Cache hit

### TC-CEC-C002: Cache invalidation
**Description**: Admin updates refresh cache  
**Test Steps**:
1. Admin updates value
2. Verify new value returned
**Expected Result**: Fresh data

### TC-CEC-C003: Cache per locale
**Description**: Separate cache per language  
**Test Steps**:
1. Call with en
2. Call with fr
**Expected Result**: Different cached responses

---

## Authorization Tests

### TC-CEC-A001: Public endpoints
**Description**: Some endpoints public  
**Expected Result**: 200 without auth

### TC-CEC-A002: Auth required for sensitive
**Description**: Some require auth  
**Expected Result**: 401 for sensitive data

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/CommonEntitiesControllerTests.cs`

