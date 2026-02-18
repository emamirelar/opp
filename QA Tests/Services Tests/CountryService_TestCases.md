# CountryService Test Cases

**Service**: `UNOPS.PAO.Business/Services/CountryService.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 20  

---

## Overview

The CountryService provides country lookup and management:
- Country CRUD operations
- Region and continent associations
- Country code validation
- Geographic filtering

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Lookup Operations | 8 | P0 |
| Search & Filter | 6 | P1 |
| Validation | 4 | P1 |
| Performance | 2 | P2 |

---

## P0 - Critical Tests

### TC-CS-001: Get all countries
**Description**: Retrieve complete list of countries  
**Test Steps**:
1. Call `GetAllAsync()`
2. Verify complete list
**Expected Result**: All active countries returned

### TC-CS-002: Get country by ID
**Description**: Retrieve country by ID  
**Test Steps**:
1. Call `GetByIdAsync(id)`
2. Verify country
**Expected Result**: Correct country returned

### TC-CS-003: Get country by ISO code
**Description**: Retrieve country by 2-letter ISO code  
**Test Steps**:
1. Call `GetByCodeAsync("KE")`
2. Verify country
**Expected Result**: Kenya returned

### TC-CS-004: Get country by 3-letter ISO code
**Description**: Retrieve country by 3-letter ISO code  
**Test Steps**:
1. Call `GetByCode3Async("KEN")`
2. Verify country
**Expected Result**: Kenya returned

### TC-CS-005: Get countries by region
**Description**: Filter countries by region  
**Test Steps**:
1. Call `GetByRegionAsync("East Africa")`
2. Verify filtered results
**Expected Result**: Only East African countries

### TC-CS-006: Get countries by continent
**Description**: Filter countries by continent  
**Test Steps**:
1. Call `GetByContinentAsync("Africa")`
2. Verify filtered results
**Expected Result**: Only African countries

### TC-CS-007: Search countries by name
**Description**: Search countries with partial match  
**Test Steps**:
1. Call `SearchAsync("Ken")`
2. Verify results
**Expected Result**: Kenya in results

### TC-CS-008: Validate country code
**Description**: Check if country code is valid  
**Test Steps**:
1. Call `IsValidCodeAsync("KE")` - valid
2. Call `IsValidCodeAsync("XX")` - invalid
**Expected Result**: true/false correctly

---

## P1 - High Priority Tests

### TC-CS-009: Get countries for dropdown
**Description**: Get simplified list for UI dropdowns  
**Test Steps**:
1. Call `GetForDropdownAsync()`
2. Verify format
**Expected Result**: ID, code, name pairs

### TC-CS-010: Get UNOPS operational countries
**Description**: Get countries where UNOPS operates  
**Test Steps**:
1. Call `GetOperationalCountriesAsync()`
2. Verify filtered list
**Expected Result**: Only operational countries

### TC-CS-011: Cache countries list
**Description**: Countries list cached for performance  
**Test Steps**:
1. Get countries (populates cache)
2. Get countries again
3. Verify cache hit
**Expected Result**: Second call faster

### TC-CS-012: Sort countries by name
**Description**: Countries sorted alphabetically  
**Test Steps**:
1. Call `GetAllAsync(sortBy: "name")`
2. Verify sort order
**Expected Result**: A-Z order

### TC-CS-013: Get country with region details
**Description**: Include region/continent in response  
**Test Steps**:
1. Call `GetByIdWithDetailsAsync(id)`
2. Verify nested data
**Expected Result**: Region and continent included

### TC-CS-014: Handle unknown country code
**Description**: Unknown code returns null  
**Test Steps**:
1. Call `GetByCodeAsync("ZZ")`
2. Verify null
**Expected Result**: null, no exception

---

## Validation Tests

### TC-CS-V001: ISO code format validation
**Description**: ISO codes must be correct format

### TC-CS-V002: Country name required
**Description**: Country name cannot be empty

### TC-CS-V003: Unique code constraint
**Description**: Duplicate codes rejected

### TC-CS-V004: Region must exist
**Description**: Region must be valid reference

---

## Performance Tests

### TC-CS-P001: Get all countries < 200ms
**Performance Criteria**: Complete in < 200ms

### TC-CS-P002: Search countries < 100ms
**Performance Criteria**: Search in < 100ms

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/CountryServiceTests.cs`

