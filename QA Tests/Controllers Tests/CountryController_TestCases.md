# CountryController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Locations/CountryController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 20  

---

## Overview

The CountryController manages country data:
- Country listing and search
- Region and continent associations
- Country code lookups
- Geographic data

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Listing & Lookup | 8 | P0 |
| Search & Filter | 6 | P1 |
| CRUD Operations | 4 | P1 |
| Authorization | 2 | P0 |

---

## P0 - Critical Tests

### TC-CC-001: Get all countries
**Description**: List all countries  
**Test Steps**:
1. Call `GET /api/countries`
**Expected Result**: All countries listed

### TC-CC-002: Get country by ID
**Description**: Get specific country  
**Test Steps**:
1. Call `GET /api/countries/{id}`
**Expected Result**: Country details

### TC-CC-003: Get country by code
**Description**: Lookup by ISO code  
**Test Steps**:
1. Call `GET /api/countries/code/{code}`
**Expected Result**: Country matching code

### TC-CC-004: Get countries for dropdown
**Description**: Simplified list for UI  
**Test Steps**:
1. Call `GET /api/countries/dropdown`
**Expected Result**: ID/code/name pairs

### TC-CC-005: Get countries by region
**Description**: Filter by region  
**Test Steps**:
1. Call `GET /api/countries?region=East Africa`
**Expected Result**: Filtered countries

### TC-CC-006: Get countries by continent
**Description**: Filter by continent  
**Test Steps**:
1. Call `GET /api/countries?continent=Africa`
**Expected Result**: Filtered countries

### TC-CC-007: Search countries
**Description**: Search by name  
**Test Steps**:
1. Call `GET /api/countries?search=Ken`
**Expected Result**: Matching countries

### TC-CC-008: Get UNOPS countries
**Description**: Countries with UNOPS presence  
**Test Steps**:
1. Call `GET /api/countries/unops`
**Expected Result**: Operational countries

---

## P1 - High Priority Tests

### TC-CC-009: Pagination support
**Description**: Paginate results  
**Test Steps**:
1. Call with page params
**Expected Result**: Paginated response

### TC-CC-010: Sort by name
**Description**: Alphabetical sort  
**Test Steps**:
1. Call with sortBy=name
**Expected Result**: A-Z order

### TC-CC-011: Sort by code
**Description**: Sort by ISO code  
**Test Steps**:
1. Call with sortBy=code
**Expected Result**: Code order

### TC-CC-012: Get regions
**Description**: List all regions  
**Test Steps**:
1. Call `GET /api/countries/regions`
**Expected Result**: Region list

### TC-CC-013: Get continents
**Description**: List all continents  
**Test Steps**:
1. Call `GET /api/countries/continents`
**Expected Result**: Continent list

### TC-CC-014: Typeahead search
**Description**: Quick search for UI  
**Test Steps**:
1. Call `GET /api/countries/typeahead?q=Ke`
**Expected Result**: Suggestions

### TC-CC-015: Create country (admin)
**Description**: Add new country  
**Test Steps**:
1. Call `POST /api/countries` as admin
**Expected Result**: Country created

### TC-CC-016: Update country (admin)
**Description**: Update country data  
**Test Steps**:
1. Call `PUT /api/countries/{id}`
**Expected Result**: Country updated

### TC-CC-017: Delete country (admin)
**Description**: Remove country  
**Test Steps**:
1. Call `DELETE /api/countries/{id}`
**Expected Result**: Country deleted

### TC-CC-018: Validate country code format
**Description**: ISO code validation  
**Test Steps**:
1. Create with invalid code
**Expected Result**: Validation error

---

## Authorization Tests

### TC-CC-A001: Read requires auth
**Expected Result**: 401 without auth

### TC-CC-A002: Write requires admin
**Expected Result**: 403 for non-admin

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/CountryControllerTests.cs`

