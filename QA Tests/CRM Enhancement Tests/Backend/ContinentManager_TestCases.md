# ContinentManager Test Cases

**Manager**: `UNOPS.PAO.UNOPSBusiness/Managers/ContinentManager.cs`  
**Entity**: `UNOPS.PAO.Domain/Entities/Continent.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 18  

---

## Overview

The ContinentManager manages continent reference data:
- Continent CRUD operations
- Region associations
- Geographic hierarchy root

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 8 | P0 |
| Associations | 5 | P1 |
| Lookups | 3 | P1 |
| Validation | 2 | P0 |

---

## P0 - Critical Tests

### TC-CM-001: Create continent
**Description**: Create new continent  
**Test Steps**:
1. Call `CreateAsync(continentData)`
**Expected Result**: Continent created

### TC-CM-002: Get continent by ID
**Description**: Retrieve continent  
**Test Steps**:
1. Create continent
2. Call `GetByIdAsync(id)`
**Expected Result**: Continent returned

### TC-CM-003: Update continent
**Description**: Update continent  
**Test Steps**:
1. Create continent
2. Call `UpdateAsync(id, data)`
**Expected Result**: Continent updated

### TC-CM-004: Delete continent
**Description**: Remove continent  
**Test Steps**:
1. Create continent
2. Call `DeleteAsync(id)`
**Expected Result**: Continent deleted

### TC-CM-005: Get all continents
**Description**: List all  
**Test Steps**:
1. Call `GetAllAsync()`
**Expected Result**: Continent list (7 expected)

### TC-CM-006: Get by code
**Description**: Lookup by code  
**Test Steps**:
1. Call `GetByCodeAsync("AF")`
**Expected Result**: Africa continent

### TC-CM-007: Name required
**Description**: Name validation  
**Test Steps**:
1. Create without name
**Expected Result**: Validation error

### TC-CM-008: Unique code
**Description**: Code must be unique  
**Test Steps**:
1. Create with existing code
**Expected Result**: Conflict error

---

## P1 - High Priority Tests

### TC-CM-009: Get regions
**Description**: Regions in continent  
**Test Steps**:
1. Create with regions
2. Call `GetRegionsAsync(continentId)`
**Expected Result**: Region list

### TC-CM-010: Count regions
**Description**: Region count  
**Test Steps**:
1. Get count
**Expected Result**: Correct count

### TC-CM-011: Count countries
**Description**: Countries via regions  
**Test Steps**:
1. Get country count
**Expected Result**: Aggregated count

### TC-CM-012: Delete with regions
**Description**: Can't delete if has regions  
**Test Steps**:
1. Create with regions
2. Try delete
**Expected Result**: Conflict error

### TC-CM-013: Get for dropdown
**Description**: Simplified list  
**Test Steps**:
1. Call dropdown endpoint
**Expected Result**: ID/name pairs

### TC-CM-014: Sort by name
**Description**: Alphabetical  
**Test Steps**:
1. Get sorted
**Expected Result**: A-Z order

### TC-CM-015: Get with statistics
**Description**: Include counts  
**Test Steps**:
1. Get with stats
**Expected Result**: Region/country counts

### TC-CM-016: Seed data
**Description**: Default 7 continents  
**Test Steps**:
1. Seed database
2. Verify count
**Expected Result**: 7 continents

---

## Validation Tests

### TC-CM-V001: Code format
**Description**: 2-char uppercase code  
**Test Steps**:
1. Create with invalid code
**Expected Result**: Validation error

### TC-CM-V002: Name max length
**Description**: 50 char limit  
**Test Steps**:
1. Create with long name
**Expected Result**: Validation error

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/ContinentManagerTests.cs`

