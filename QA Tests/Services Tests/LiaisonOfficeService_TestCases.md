# LiaisonOfficeService Test Cases

**Service**: `UNOPS.PAO.Business/Services/LiaisonOfficeService.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 25  

---

## Overview

The LiaisonOfficeService manages liaison office data:
- Liaison office CRUD operations
- Partner associations
- Geographic filtering
- Office lookups

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 10 | P0 |
| Partner Associations | 7 | P0 |
| Search & Filter | 5 | P1 |
| Validation | 3 | P1 |

---

## P0 - Critical Tests

### TC-LOS-001: Create liaison office
**Description**: Create new liaison office  
**Test Steps**:
1. Call `CreateAsync(officeData)`
2. Verify office created
**Expected Result**: Office with generated ID

### TC-LOS-002: Get liaison office by ID
**Description**: Retrieve office by ID  
**Test Steps**:
1. Create office
2. Call `GetByIdAsync(id)`
3. Verify data
**Expected Result**: Correct office returned

### TC-LOS-003: Update liaison office
**Description**: Update existing office  
**Test Steps**:
1. Create office
2. Call `UpdateAsync(id, updatedData)`
3. Verify changes
**Expected Result**: Office updated

### TC-LOS-004: Delete liaison office (soft)
**Description**: Soft delete office  
**Test Steps**:
1. Create office
2. Call `DeleteAsync(id)`
3. Verify marked deleted
**Expected Result**: IsDeleted = true

### TC-LOS-005: Get office by code
**Description**: Retrieve by unique code  
**Test Steps**:
1. Create office with code
2. Call `GetByCodeAsync(code)`
**Expected Result**: Correct office

### TC-LOS-006: Get all liaison offices
**Description**: Retrieve all active offices  
**Test Steps**:
1. Create multiple offices
2. Call `GetAllAsync()`
**Expected Result**: All active offices

### TC-LOS-007: Associate partner with office
**Description**: Link partner to office  
**Test Steps**:
1. Call `AssociatePartnerAsync(officeId, partnerId)`
2. Verify association
**Expected Result**: Partner linked

### TC-LOS-008: Remove partner association
**Description**: Unlink partner from office  
**Test Steps**:
1. Create association
2. Call `RemovePartnerAsync(officeId, partnerId)`
**Expected Result**: Association removed

### TC-LOS-009: Get partners by office
**Description**: Get all partners for office  
**Test Steps**:
1. Associate multiple partners
2. Call `GetPartnersAsync(officeId)`
**Expected Result**: All associated partners

### TC-LOS-010: Get office for partner
**Description**: Get partner's liaison office  
**Test Steps**:
1. Associate partner
2. Call `GetOfficeForPartnerAsync(partnerId)`
**Expected Result**: Correct office

---

## P1 - High Priority Tests

### TC-LOS-011: Search offices by name
**Description**: Search with name filter  
**Test Steps**:
1. Call `SearchAsync("Regional")`
**Expected Result**: Matching offices

### TC-LOS-012: Filter by country
**Description**: Filter offices by country  
**Test Steps**:
1. Call `GetByCountryAsync("KE")`
**Expected Result**: Kenya offices only

### TC-LOS-013: Filter by region
**Description**: Filter offices by region  
**Test Steps**:
1. Call `GetByRegionAsync("Africa")`
**Expected Result**: Africa offices only

### TC-LOS-014: Get offices by org unit
**Description**: Filter by organization unit  
**Test Steps**:
1. Call `GetByOrgUnitAsync(orgUnitId)`
**Expected Result**: Org unit offices

### TC-LOS-015: Typeahead search
**Description**: Quick search for autocomplete  
**Test Steps**:
1. Call `TypeaheadAsync("Nai", limit: 5)`
**Expected Result**: Top 5 matches

### TC-LOS-016: Cannot delete office with partners
**Description**: Delete blocked if partners exist  
**Test Steps**:
1. Create office with partners
2. Attempt delete
**Expected Result**: Error - office in use

### TC-LOS-017: Duplicate code prevented
**Description**: Unique code constraint  
**Test Steps**:
1. Create office with code
2. Try create with same code
**Expected Result**: Conflict error

---

## Validation Tests

### TC-LOS-V001: Code format validation
**Description**: Office code format enforced

### TC-LOS-V002: Required fields validation
**Description**: Name and code required

### TC-LOS-V003: Email format validation
**Description**: Contact email format validated

---

## Performance Tests

### TC-LOS-P001: Get all offices < 300ms
**Performance Criteria**: < 300ms for 500 offices

### TC-LOS-P002: Search offices < 200ms
**Performance Criteria**: < 200ms for search

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/LiaisonOfficeServiceTests.cs`

