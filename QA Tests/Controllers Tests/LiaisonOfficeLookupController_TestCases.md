# LiaisonOfficeLookupController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/LiaisonOffices/LiaisonOfficeLookupController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 18  

---

## Overview

The LiaisonOfficeLookupController provides lookup operations for liaison offices:
- Quick lookup by ID/code
- Typeahead search for dropdowns
- Filtered listings for UI components

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Lookup Operations | 8 | P0 |
| Typeahead/Search | 6 | P1 |
| Authorization | 4 | P0 |

---

## P0 - Critical Tests

### TC-LOLC-001: Get liaison office by ID
**Description**: Retrieve office by ID  
**Test Steps**:
1. Call `GET /api/liaison-offices/lookup/{id}`
**Expected Result**: Office details returned

### TC-LOLC-002: Get liaison office by code
**Description**: Retrieve by code  
**Test Steps**:
1. Call `GET /api/liaison-offices/lookup/code/{code}`
**Expected Result**: Matching office

### TC-LOLC-003: Get all for dropdown
**Description**: Simplified list for UI  
**Test Steps**:
1. Call `GET /api/liaison-offices/lookup/dropdown`
**Expected Result**: ID/name pairs

### TC-LOLC-004: Get by ID - not found
**Description**: Non-existing ID  
**Test Steps**:
1. Call with invalid ID
**Expected Result**: 404 Not Found

### TC-LOLC-005: Get by code - not found
**Description**: Non-existing code  
**Test Steps**:
1. Call with invalid code
**Expected Result**: 404 Not Found

### TC-LOLC-006: Get active offices only
**Description**: Filter by status  
**Test Steps**:
1. Call `GET /api/liaison-offices/lookup?status=active`
**Expected Result**: Only active offices

### TC-LOLC-007: Get by country
**Description**: Filter by country  
**Test Steps**:
1. Call with countryId parameter
**Expected Result**: Offices in country

### TC-LOLC-008: Get by region
**Description**: Filter by region  
**Test Steps**:
1. Call with regionId parameter
**Expected Result**: Offices in region

---

## P1 - High Priority Tests

### TC-LOLC-009: Typeahead search
**Description**: Quick search for autocomplete  
**Test Steps**:
1. Call `GET /api/liaison-offices/lookup/typeahead?q=Cop`
**Expected Result**: Matching suggestions

### TC-LOLC-010: Typeahead - minimum chars
**Description**: Require minimum input  
**Test Steps**:
1. Call with single character
**Expected Result**: Empty or error

### TC-LOLC-011: Typeahead - result limit
**Description**: Limit results  
**Test Steps**:
1. Call with common prefix
**Expected Result**: Max 10 results

### TC-LOLC-012: Search with filters
**Description**: Combined search and filter  
**Test Steps**:
1. Call with q and country filter
**Expected Result**: Filtered matches

### TC-LOLC-013: Sort by name
**Description**: Alphabetical ordering  
**Test Steps**:
1. Call with sortBy=name
**Expected Result**: A-Z order

### TC-LOLC-014: Include inactive
**Description**: Show all statuses  
**Test Steps**:
1. Call with includeInactive=true
**Expected Result**: All offices

---

## Authorization Tests

### TC-LOLC-A001: Unauthenticated denied
**Expected Result**: 401 Unauthorized

### TC-LOLC-A002: Authenticated access
**Expected Result**: 200 OK

### TC-LOLC-A003: Org unit filter applied
**Expected Result**: Only permitted offices

### TC-LOLC-A004: Admin sees all
**Expected Result**: No filtering for admin

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/LiaisonOfficeLookupControllerTests.cs`

