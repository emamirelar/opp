# GeoRegionManager Test Cases

**Manager**: `UNOPS.PAO.UNOPSBusiness/Managers/GeoRegionManager.cs`  
**Entity**: `UNOPS.PAO.Domain/Entities/GeoRegion.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 22  

---

## Overview

The GeoRegionManager manages geographic regions:
- Region CRUD operations
- Continent associations
- Country mappings
- Hierarchical organization

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 8 | P0 |
| Hierarchy | 6 | P1 |
| Associations | 5 | P1 |
| Validation | 3 | P0 |

---

## P0 - Critical Tests

### TC-GR-001: Create region
**Description**: Create new geographic region  
**Test Steps**:
1. Call `CreateAsync(regionData)`
**Expected Result**: Region created

### TC-GR-002: Get region by ID
**Description**: Retrieve region  
**Test Steps**:
1. Create region
2. Call `GetByIdAsync(id)`
**Expected Result**: Region returned

### TC-GR-003: Update region
**Description**: Update region details  
**Test Steps**:
1. Create region
2. Call `UpdateAsync(id, data)`
**Expected Result**: Region updated

### TC-GR-004: Delete region
**Description**: Remove region  
**Test Steps**:
1. Create region
2. Call `DeleteAsync(id)`
**Expected Result**: Region deleted

### TC-GR-005: Get all regions
**Description**: List all regions  
**Test Steps**:
1. Create regions
2. Call `GetAllAsync()`
**Expected Result**: Region list

### TC-GR-006: Get by code
**Description**: Lookup by code  
**Test Steps**:
1. Create with code
2. Call `GetByCodeAsync(code)`
**Expected Result**: Matching region

### TC-GR-007: Name required
**Description**: Name validation  
**Test Steps**:
1. Create without name
**Expected Result**: Validation error

### TC-GR-008: Unique code
**Description**: Code must be unique  
**Test Steps**:
1. Create with code
2. Create duplicate code
**Expected Result**: Conflict error

---

## P1 - High Priority Tests

### TC-GR-009: Set continent
**Description**: Associate with continent  
**Test Steps**:
1. Create region
2. Set continentId
**Expected Result**: Association set

### TC-GR-010: Get by continent
**Description**: Regions in continent  
**Test Steps**:
1. Create with continent
2. Call `GetByContinentIdAsync(continentId)`
**Expected Result**: Filtered regions

### TC-GR-011: Get countries in region
**Description**: List region's countries  
**Test Steps**:
1. Assign countries
2. Call `GetCountriesAsync(regionId)`
**Expected Result**: Country list

### TC-GR-012: Count countries
**Description**: Country count per region  
**Test Steps**:
1. Assign countries
2. Get count
**Expected Result**: Correct count

### TC-GR-013: Get for dropdown
**Description**: Simplified list  
**Test Steps**:
1. Call `GetForDropdownAsync()`
**Expected Result**: ID/name pairs

### TC-GR-014: Search by name
**Description**: Name search  
**Test Steps**:
1. Search partial name
**Expected Result**: Matching regions

### TC-GR-015: Sort alphabetically
**Description**: Name ordering  
**Test Steps**:
1. Get sorted
**Expected Result**: A-Z order

### TC-GR-016: Get with countries
**Description**: Include related data  
**Test Steps**:
1. Get with includes
**Expected Result**: Countries included

### TC-GR-017: Get hierarchy
**Description**: Continent > Region structure  
**Test Steps**:
1. Get hierarchical view
**Expected Result**: Nested structure

### TC-GR-018: Move to continent
**Description**: Change continent  
**Test Steps**:
1. Update continentId
**Expected Result**: Moved

### TC-GR-019: Delete with countries
**Description**: Can't delete if has countries  
**Test Steps**:
1. Create with countries
2. Try delete
**Expected Result**: Conflict error

---

## Validation Tests

### TC-GR-V001: Code format
**Description**: Code must be uppercase  
**Test Steps**:
1. Create with lowercase
**Expected Result**: Normalized or error

### TC-GR-V002: Code max length
**Description**: 10 char limit  
**Test Steps**:
1. Create with long code
**Expected Result**: Validation error

### TC-GR-V003: Name max length
**Description**: 100 char limit  
**Test Steps**:
1. Create with long name
**Expected Result**: Validation error

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/GeoRegionManagerTests.cs`

